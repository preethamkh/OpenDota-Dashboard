using DotaDashboard.Data;
using DotaDashboard.Helpers;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotaDashboard.Services.Implementation;

public class JobService : IJobService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JobService> _logger;
    private readonly RabbitMqHelper _rabbitMqHelper;

    public JobService(ApplicationDbContext context, ILogger<JobService> logger, RabbitMqHelper rabbitMqHelper)
    {
        _context = context;
        _logger = logger;
        _rabbitMqHelper = rabbitMqHelper;
    }

    public async Task<Job> CreateJobAsync(string jobType, string? target = null)
    {
        try
        {
            _logger.LogInformation("Creating job of type {JobType} with target {Target}", jobType, target);

            var job = new Job
            {
                Type = jobType,
                Status = JobStatus.Pending,
                Target = target,
                MatchesProcessed = 0,
                Retries = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Publish job to RabbitMQ queue
            _rabbitMqHelper.PublishJob(job);

            _logger.LogInformation("Job {JobId} created and published to queue", job.JobId);
            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job of type {JobType}", jobType);
            throw;
        }
    }

    public async Task UpdateJobStatusAsync(int jobId, string status, string? error = null)
    {
        try
        {
            var job = await GetJobByIdAsync(jobId);
            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found", jobId);
                return;
            }

            job.Status = status;
            job.Error = error;
            job.UpdatedAt = DateTime.UtcNow;

            if (status == JobStatus.Done || status == JobStatus.Failed)
            {
                job.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Job {JobId} status updated to {Status}", jobId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId} status", jobId);
            throw;
        }
    }

    public async Task UpdateJobProgressAsync(int jobId, int matchesProcessed)
    {
        try
        {
            var job = await GetJobByIdAsync(jobId);
            if (job == null) return;

            job.MatchesProcessed = matchesProcessed;
            job.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId} progress", jobId);
        }
    }

    public async Task CompleteJobAsync(int jobId, int matchesProcessed)
    {
        try
        {
            var job = await GetJobByIdAsync(jobId);
            if (job == null) return;

            job.Status = JobStatus.Done;
            job.MatchesProcessed = matchesProcessed;
            job.CompletedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Job {JobId} completed successfully with {Count} matches processed",
                jobId, matchesProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing job {JobId}", jobId);
            throw;
        }
    }

    public async Task FailJobAsync(int jobId, string error)
    {
        try
        {
            var job = await GetJobByIdAsync(jobId);
            if (job == null) return;

            job.Status = JobStatus.Failed;
            job.Error = error;
            job.CompletedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogError("Job {JobId} failed: {Error}", jobId, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking job {JobId} as failed", jobId);
            throw;
        }
    }

    public async Task<Job?> GetJobByIdAsync(int jobId)
    {
        return await _context.Jobs.FindAsync(jobId);
    }

    public async Task<List<Job>> GetJobsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Jobs
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Job>> GetPendingJobsAsync()
    {
        return await _context.Jobs
            .Where(j => j.Status == JobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Job>> GetRunningJobsAsync()
    {
        return await _context.Jobs
            .Where(j => j.Status == JobStatus.Running)
            .ToListAsync();
    }

    public async Task<Job> RetryJobAsync(int jobId)
    {
        try
        {
            var job = await GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new InvalidOperationException($"Job {jobId} not found");
            }

            job.Status = JobStatus.Pending;
            job.Retries++;
            job.Error = null;
            job.UpdatedAt = DateTime.UtcNow;
            job.CompletedAt = null;

            await _context.SaveChangesAsync();

            // Re-publish to queue
            _rabbitMqHelper.PublishJob(job);

            _logger.LogInformation("Job {JobId} retried (retry count: {Retries})", jobId, job.Retries);
            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying job {JobId}", jobId);
            throw;
        }
    }

    public async Task<int> GetActiveJobCountAsync()
    {
        return await _context.Jobs
            .Where(j => j.Status == JobStatus.Pending || j.Status == JobStatus.Running)
            .CountAsync();
    }
}
