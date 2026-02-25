namespace Ecommerce.Notification.API.Messaging
{
    /// <summary>
    /// Interface for consuming events from message broker
    /// </summary>
    public interface IEventConsumer
    {
        /// <summary>
        /// Start consuming events
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop consuming events
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
