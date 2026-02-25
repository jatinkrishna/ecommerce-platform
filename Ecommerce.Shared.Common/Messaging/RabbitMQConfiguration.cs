namespace Ecommerce.Shared.Common.Messaging
{
    /// <summary>
    /// Configuration for RabbitMQ connection
    /// </summary>
    public class RabbitMQConfiguration
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "ecommerce.events";
        public string ExchangeType { get; set; } = "topic";
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// Get connection string for RabbitMQ
        /// </summary>
        public string GetConnectionString()
        {
            return $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
        }
    }
}
