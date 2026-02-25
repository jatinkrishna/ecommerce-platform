using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Ecommerce.Shared.Common.Messaging
{
    /// <summary>
    /// RabbitMQ implementation of event publisher
    /// </summary>
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private readonly RabbitMQConfiguration _config;
        private readonly ILogger<RabbitMQEventPublisher> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public RabbitMQEventPublisher(
            RabbitMQConfiguration config,
            ILogger<RabbitMQEventPublisher> logger)
        {
            _config = config;
            _logger = logger;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config.Host,
                    Port = _config.Port,
                    UserName = _config.Username,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Enable publisher confirms
                _channel.ConfirmSelect();

                // Declare exchange
                _channel.ExchangeDeclare(
                    exchange: _config.ExchangeName,
                    type: _config.ExchangeType,
                    durable: _config.Durable,
                    autoDelete: _config.AutoDelete,
                    arguments: null);

                _logger.LogInformation(
                    "RabbitMQ connection established. Exchange: {Exchange}, Type: {Type}",
                    _config.ExchangeName,
                    _config.ExchangeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        public Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null) where TEvent : class
        {
            return Task.Run(() =>
            {
                try
                {
                    EnsureConnection();

                    var eventType = @event.GetType().Name;
                    var key = routingKey ?? eventType.ToLower();

                    var message = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None,
                        Formatting = Formatting.None
                    });

                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = _channel!.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
                    properties.Type = eventType;
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    lock (_lock)
                    {
                        _channel.BasicPublish(
                            exchange: _config.ExchangeName,
                            routingKey: key,
                            basicProperties: properties,
                            body: body);

                        // Wait for confirmation
                        _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
                    }

                    _logger.LogInformation(
                        "Published event {EventType} with routing key {RoutingKey}",
                        eventType,
                        key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to publish event {EventType}",
                        @event.GetType().Name);
                    throw;
                }
            });
        }

        public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, string? routingKey = null) where TEvent : class
        {
            var tasks = events.Select(e => PublishAsync(e, routingKey));
            await Task.WhenAll(tasks);
        }

        private void EnsureConnection()
        {
            if (_channel == null || !_channel.IsOpen || _connection == null || !_connection.IsOpen)
            {
                _logger.LogWarning("RabbitMQ connection lost. Reconnecting...");
                InitializeRabbitMQ();
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ connection closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
