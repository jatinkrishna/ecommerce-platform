using Ecommerce.Notification.API.Messaging;

namespace Ecommerce.Notification.API.Services
{
    /// <summary>
    /// Background service that runs the event consumer
    /// </summary>
    public class EventConsumerHostedService : BackgroundService
    {
        private readonly IEventConsumer _eventConsumer;
        private readonly ILogger<EventConsumerHostedService> _logger;

        public EventConsumerHostedService(
            IEventConsumer eventConsumer,
            ILogger<EventConsumerHostedService> logger)
        {
            _eventConsumer = eventConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Event Consumer Hosted Service is starting");

            // Delay startup to allow other services to initialize
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            try
            {
                _logger.LogInformation("Attempting to start event consumer...");
                await _eventConsumer.StartAsync(stoppingToken);
                _logger.LogInformation("Event consumer started successfully");

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Event consumer is shutting down");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event Consumer Hosted Service encountered an error: {Message}", ex.Message);
                // Don't throw - let the service continue running
                // The consumer will retry connection automatically
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event Consumer Hosted Service is stopping");
            
            await _eventConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
