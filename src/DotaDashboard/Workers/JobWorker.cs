using DotaDashboard.Helpers;
using DotaDashboard.Models;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace DotaDashboard.Workers;

/// <summary>
/// Background service that processes jobs from the RabbitMQ queue
/// </summary>
public class JobWorker(
    IServiceProvider serviceProvider,
    ILogger<JobWorker> logger,
    IConfiguration configuration)
    : BackgroundService
{
    private RabbitMqHelper? _rabbitMqHelper;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check if worker is enabled
        var workerEnabled = configuration.GetValue("JobSettings:WorkerEnabled", true);

        if (!workerEnabled)
        {
            logger.LogInformation("Job worker is disabled in configuration");
            return;
        }

        logger.LogInformation("Job worker starting...");
        
        // Create RabbitMQ helper in the worker scope
        using var scope = serviceProvider.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var rabbitMqLogger = loggerFactory.CreateLogger<RabbitMqHelper>();

        _rabbitMqHelper = new RabbitMqHelper(configuration, rabbitMqLogger);

        var consumer = _rabbitMqHelper.CreateConsumer();

        if (consumer == null)
        {
            logger.LogError("Failed to create RabbitMQ consumer. Worker cannot start.");
            return;
        }

        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            logger.LogInformation("Received job message: {Message}", message);

            try
            {
                var jobMessage = JsonConvert.DeserializeObject<JobMessage>(message);
                if (jobMessage == null)
                {
                    logger.LogError("Failed to deserialize job message");
                    _rabbitMqHelper.AckMessage(ea.DeliveryTag);
                    return;
                }

                // Process the job in a new scope
                await ProcessJobAsync(jobMessage, stoppingToken);

                // Acknowledge successful processing
                _rabbitMqHelper.AckMessage(ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing job message. Message will be re-queued.");

                // Negative acknowledge - requeue the message
                _rabbitMqHelper.NAckMessage(ea.DeliveryTag, requeue: true);
            }
        };

        _rabbitMqHelper.StartConsuming(consumer);

        logger.LogInformation("Job worker started and listening for messages");

        // Keep the worker running
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Job worker stopping...");
        }
    }

    private async Task ProcessJobAsync(JobMessage jobMessage, CancellationToken _)
    {
        using var scope = serviceProvider.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
        var dataIngestionService = scope.ServiceProvider.GetRequiredService<IDataIngestionService>();

        try
        {
            logger.LogInformation("Processing job {JobId} of type {Type}", jobMessage.JobId, jobMessage.Type);

            // Update job status to Running
            await jobService.UpdateJobStatusAsync(jobMessage.JobId, JobStatus.Running);

            int matchesProcessed;

            // Process based on job type
            switch (jobMessage.Type)
            {
                case JobType.IngestMatches:
                    var limit = configuration.GetValue("JobSettings:DefaultMatchesToFetch", 50);
                    matchesProcessed = await dataIngestionService.IngestProMatchesAsync(limit);
                    break;

                case JobType.IngestHeroes:
                    matchesProcessed = await dataIngestionService.IngestHeroesAsync();
                    break;

                case JobType.AggregateStats:
                    // For aggregate stats, we don't ingest matches, just recalculate
                    // This can be implemented later if needed
                    logger.LogInformation("AggregateStats job type - no action needed (stats calculated on-demand)");
                    matchesProcessed = 0;

                    // todo: later
                    // Actually calculate/aggregate statistics
                    //var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
                    //matchesProcessed = await statsService.RecalculateAggregatesAsync();
                    break;

                default:
                    logger.LogWarning("Unknown job type: {Type}", jobMessage.Type);
                    await jobService.FailJobAsync(jobMessage.JobId, $"Unknown job type: {jobMessage.Type}");
                    return;
            }

            // Mark job as completed
            await jobService.CompleteJobAsync(jobMessage.JobId, matchesProcessed);

            logger.LogInformation("Job {JobId} completed successfully. Processed {Count} items",
                jobMessage.JobId, matchesProcessed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing job {JobId}", jobMessage.JobId);

            // Check retry count
            var job = await jobService.GetJobByIdAsync(jobMessage.JobId);
            var maxRetries = configuration.GetValue("JobSettings:MaxRetries", 3);

            if (job != null && job.Retries < maxRetries)
            {
                // todo: Increment retry count before re-queuing
                //await jobService.UpdateJobRetryCountAsync(jobMessage.JobId, job.Retries + 1);

                logger.LogInformation("Job {JobId} will be retried (attempt {Retries}/{MaxRetries})",
                    jobMessage.JobId, job.Retries + 1, maxRetries);

                // Will be retried by re-queuing
                throw;
            }
            else
            {
                // Max retries reached, mark as failed
                await jobService.FailJobAsync(jobMessage.JobId, ex.Message);
            }
        }
    }

    public override void Dispose()
    {
        _rabbitMqHelper?.Dispose();
        base.Dispose();
    }
}
