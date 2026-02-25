namespace Ecommerce.Shared.Common.Events
{
    /// <summary>
    /// Event published when a user successfully logs in
    /// </summary>
    public class UserLoggedInEvent : BaseEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime LoginAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
