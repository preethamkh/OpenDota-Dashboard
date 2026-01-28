using DotaDashboard.Models.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DotaDashboard.Helpers;

/// <summary>
/// Helper class for RabbitMQ operations
/// </summary>
public class RabbitMqHelper : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqHelper> _logger;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly string _queueName;

    public RabbitMqHelper(IConfiguration configuration, ILogger<RabbitMqHelper> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _queueName = configuration["RabbitMQ:QueueName"] ?? "opendota-jobs";

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
                Password = _configuration["RabbitMQ:Password"] ?? "admin123",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                _queueName,
                // survive broker restarts
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments:null);

            _logger.LogInformation("RabbitMQ connection established. Queue: {QueueName}", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            _connection = null;
            _channel = null;
        }
    }

    public void PublishJob(Job job)
    {
        try
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogError("RabbitMQ channel is not available. Job {JobId} not published.", job.JobId);
                return;
            }

            var message = JsonConvert.SerializeObject(new JobMessage)
        }
        catch (Exception ex)
        {
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
