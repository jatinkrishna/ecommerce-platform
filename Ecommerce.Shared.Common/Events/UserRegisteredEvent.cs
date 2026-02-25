namespace Ecommerce.Shared.Common.Events
{
    /// <summary>
    /// Event published when a new user successfully registers
    /// </summary>
    public class UserRegisteredEvent : BaseEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
