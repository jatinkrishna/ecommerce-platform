namespace Ecommerce.Shared.Common.Messaging
{
    /// <summary>
    /// Interface for publishing events to message broker
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish an event to the message broker
        /// </summary>
        /// <typeparam name="TEvent">Type of event to publish</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="routingKey">Optional routing key for targeted delivery</param>
        /// <returns>Task representing the publish operation</returns>
        Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null) where TEvent : class;

        /// <summary>
        /// Publish multiple events in a batch
        /// </summary>
        Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, string? routingKey = null) where TEvent : class;
    }
}
