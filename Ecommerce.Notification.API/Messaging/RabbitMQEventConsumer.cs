using Ecommerce.Notification.API.Services;
using Ecommerce.Shared.Common.Events;
using Ecommerce.Shared.Common.Messaging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Ecommerce.Notification.API.Messaging
{
    /// <summary>
    /// RabbitMQ implementation of event consumer
    /// </summary>
    public class RabbitMQEventConsumer : IEventConsumer, IDisposable
    {
        private readonly RabbitMQConfiguration _config;
        private readonly ILogger<RabbitMQEventConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private string? _queueName;
        private bool _disposed = false;

        public RabbitMQEventConsumer(
            RabbitMQConfiguration config,
            ILogger<RabbitMQEventConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting RabbitMQ event consumer...");
                _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", _config.Host, _config.Port);

                // Create connection
                var factory = new ConnectionFactory
                {
                    HostName = _config.Host,
                    Port = _config.Port,
                    UserName = _config.Username,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    SocketReadTimeout = TimeSpan.FromSeconds(30),
                    SocketWriteTimeout = TimeSpan.FromSeconds(30)
                };

                _logger.LogInformation("Creating RabbitMQ connection...");
                _connection = factory.CreateConnection();
                _logger.LogInformation("RabbitMQ connection established");

                _channel = _connection.CreateModel();
                _logger.LogInformation("RabbitMQ channel created");

                // Declare exchange (must exist)
                _channel.ExchangeDeclare(
                    exchange: _config.ExchangeName,
                    type: _config.ExchangeType,
                    durable: _config.Durable,
                    autoDelete: _config.AutoDelete);
                _logger.LogInformation("Exchange declared: {Exchange}", _config.ExchangeName);

                // Declare queue for notification service
                _queueName = "notification.service.queue";
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _logger.LogInformation("Queue declared: {Queue}", _queueName);

                // Bind queue to exchange with routing patterns
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _config.ExchangeName,
                    routingKey: "userregisteredevent");
                _logger.LogInformation("Queue bound to routing key: userregisteredevent");

                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _config.ExchangeName,
                    routingKey: "userlogginedevent");
                _logger.LogInformation("Queue bound to routing key: userlogginedevent");

                // Set up consumer
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    await HandleMessage(ea);
                };

                _channel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation(
                    "RabbitMQ event consumer started successfully. Queue: {Queue}, Exchange: {Exchange}",
                    _queueName,
                    _config.ExchangeName);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RabbitMQ event consumer: {Message}. Host: {Host}:{Port}", 
                    ex.Message, _config.Host, _config.Port);
                throw;
            }
        }

        private async Task HandleMessage(BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation(
                    "Received event with routing key: {RoutingKey}",
                    routingKey);

                // Process based on routing key
                if (routingKey == "userregisteredevent")
                {
                    var userRegisteredEvent = JsonConvert.DeserializeObject<UserRegisteredEvent>(message);
                    if (userRegisteredEvent != null)
                    {
                        await ProcessUserRegisteredEvent(userRegisteredEvent);
                    }
                }
                else if (routingKey == "userlogginedevent")
                {
                    var userLoggedInEvent = JsonConvert.DeserializeObject<UserLoggedInEvent>(message);
                    if (userLoggedInEvent != null)
                    {
                        await ProcessUserLoggedInEvent(userLoggedInEvent);
                    }
                }

                // Acknowledge message
                _channel?.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message");
                
                // Negative acknowledge - requeue the message
                _channel?.BasicNack(ea.DeliveryTag, false, true);
            }
        }

        private async Task ProcessUserRegisteredEvent(UserRegisteredEvent @event)
        {
            _logger.LogInformation(
                "Processing UserRegistered event for user: {UserId}, Email: {Email}, Name: {FirstName} {LastName}",
                @event.UserId,
                @event.Email,
                @event.FirstName,
                @event.LastName);

            // ========== PHASE 3 - DAY 4: Send Welcome Email ==========
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendWelcomeEmailAsync(@event.Email, @event.FirstName, @event.LastName);

                _logger.LogInformation(
                    "Welcome email sent successfully to {Email}",
                    @event.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", @event.Email);
                // Don't throw - event processing succeeded even if email failed
            }
            // ========== END PHASE 3 - DAY 4 ==========
        }

        private async Task ProcessUserLoggedInEvent(UserLoggedInEvent @event)
        {
            _logger.LogInformation(
                "Processing UserLoggedIn event for user: {UserId}, Email: {Email}",
                @event.UserId,
                @event.Email);

            // ========== PHASE 3 - DAY 4: Send Login Notification (Optional) ==========
            // For now, just log it. Can be enabled if needed.
            _logger.LogInformation("User {Email} logged in at {LoginTime}", @event.Email, @event.LoginAt);
            // ========== END PHASE 3 - DAY 4 ==========

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping RabbitMQ event consumer...");
            
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.LogInformation("RabbitMQ event consumer stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping RabbitMQ event consumer");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ event consumer");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
