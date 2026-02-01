using System.Text;
using DotaDashboard.Models;
using DotaDashboard.Models.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
            var userName = _configuration["RabbitMQ:UserName"] ?? "admin";
            var virtualHost = _configuration["RabbitMQ:VirtualHost"] ?? userName; // Default to username for CloudAMQP

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = userName,
                Password = _configuration["RabbitMQ:Password"] ?? "admin123",
                VirtualHost = virtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            // Enable SSL/TLS for cloud RabbitMQ services (CloudAMQP)
            var useSsl = _configuration.GetValue("RabbitMQ:UseSsl", false);
            if (useSsl)
            {
                factory.Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = factory.HostName,
                    AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch |
                                             System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
                };
                _logger.LogInformation("SSL/TLS enabled for RabbitMQ connection to {HostName}", factory.HostName);
            }

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

            var message = JsonConvert.SerializeObject(new JobMessage
            {
                JobId = job.JobId,
                Type = job.Type,
                Target = job.Target
            });

            // Most messaging systems use UTF-8 encoding for text messages incl. RabbitMQ
            var body = Encoding.UTF8.GetBytes(message);

            // set metadata for the RabbitMQ message
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // survive broker restarts
            properties.ContentType = "application/json";
            properties.DeliveryMode = 2; // make message persistent

            // publish the message
            _channel.BasicPublish(
                exchange: "", // default, no exchange // options are: direct, topic, headers, fanout
                routingKey: _queueName, // where to send the message
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Job {JobId} published to queue {QueueName}", job.JobId, _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing job {JobId} to queue", job.JobId);
            // todo: return a bool instead so that job is created and stays in the queue when failed. Currently, this error will propagate to the Razor page
            throw;
        }
    }

    /// <summary>
    /// Create a consumer for processing jobs
    /// </summary>
    public EventingBasicConsumer? CreateConsumer()
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogError("RabbitMQ channel is not available. Cannot create consumer.");
            return null;
        }

        // Set QoS to process one message at a time
        // prefetchSize: 0 - No size limit per message
        // prefetchCount: 1 - Worker gets 1 message at a time
        // global: false - applies per-consumer (not per channel)
        //
        // WITHOUT QoS:
        //
        // Worker 1: Gets 10 messages → Processes slowly(5 min each)
        // Worker 2: Gets 0 messages → Sits idle
        // Worker 3: Gets 0 messages → Sits idle
        //
        // WITH QoS:
        //
        // Worker 1: Gets 1 message → Processing...
        // Worker 2: Gets 1 message → Processing...
        // Worker 3: Gets 1 message → Processing...
           
        // Worker 2 finishes → Gets next message immediately
        // Worker 1 still working → No new messages until done

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        _logger.LogInformation("Consumer created for queue {QueueName}", _queueName);

        return consumer;
    }

    /// <summary>
    /// Start consuming messages
    /// </summary>
    public void StartConsuming(EventingBasicConsumer consumer)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogError("RabbitMQ channel is not available. Cannot start consuming.");
            return;
        }

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false, // Manual acknowledgment
            consumer: consumer
        );

        _logger.LogInformation("Started consuming messages from queue {QueueName}", _queueName);
    }

    /// <summary>
    /// Acknowledge message processing
    /// </summary>
    public void AckMessage(ulong deliveryTag)
    {
        if (_channel == null || !_channel.IsOpen) return;

        _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
    }

    public void NAckMessage(ulong deliveryTag, bool requeue = true)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            return;
        }

        _channel.BasicNack(deliveryTag, false, requeue);
    }

    /// <summary>
    /// Get queue message count
    /// </summary>
    public uint GetQueueMessageCount()
    {
        if (_channel == null || !_channel.IsOpen) return 0;

        var queueDeclareOk = _channel.QueueDeclarePassive(_queueName);
        return queueDeclareOk.MessageCount;
    }

    /// <summary>
    /// For the background worker to get the channel
    /// </summary>
    public IModel? GetChannel() => _channel;

    public void Dispose()
    {
        // close and then dispose??
        // sends AMQP protocol close frames to RabbitMQ server
        // cleans up gracefully
        // goodbye
        _channel?.Close();
        // hangup!
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
