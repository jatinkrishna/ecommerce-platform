namespace Ecommerce.Shared.Common.Events
{
    /// <summary>
    /// Base class for all domain events
    /// </summary>
    public abstract class BaseEvent
    {
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        public Guid EventId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// When this event was created
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Correlation ID for tracking related events
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Event type name for routing
        /// </summary>
        public string EventType => GetType().Name;
    }
}
