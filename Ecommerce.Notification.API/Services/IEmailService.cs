namespace Ecommerce.Notification.API.Services
{
    /// <summary>
    /// Interface for email service
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send welcome email to newly registered user
        /// </summary>
        Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName);

        /// <summary>
        /// Send login notification email
        /// </summary>
        Task SendLoginNotificationAsync(string toEmail, string firstName, DateTime loginTime);

        /// <summary>
        /// Send generic email
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}
