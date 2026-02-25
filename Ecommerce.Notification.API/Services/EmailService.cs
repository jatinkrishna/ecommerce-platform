using Ecommerce.Notification.API.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Ecommerce.Notification.API.Services
{
    /// <summary>
    /// Email service implementation using MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName)
        {
            var subject = "Welcome to Our E-Commerce Platform!";
            var body = GetWelcomeEmailTemplate(firstName, lastName);

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public async Task SendLoginNotificationAsync(string toEmail, string firstName, DateTime loginTime)
        {
            var subject = "New Login Detected";
            var body = GetLoginNotificationTemplate(firstName, loginTime);

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            if (!_config.Enabled)
            {
                _logger.LogInformation("Email service is disabled. Email not sent to {Email}", toEmail);
                return;
            }

            try
            {
                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", toEmail, subject);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
                message.To.Add(new MailboxAddress(string.Empty, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Connect to SMTP server
                await client.ConnectAsync(_config.SmtpServer, _config.SmtpPort, 
                    _config.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate
                if (!string.IsNullOrEmpty(_config.SmtpUsername))
                {
                    await client.AuthenticateAsync(_config.SmtpUsername, _config.SmtpPassword);
                }

                // Send email
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Message}", toEmail, ex.Message);
                throw;
            }
        }

        private string GetWelcomeEmailTemplate(string firstName, string lastName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Our Platform!</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName} {lastName}!</h2>
            <p>Thank you for joining our e-commerce platform. We're excited to have you on board!</p>
            
            <p>With your new account, you can:</p>
            <ul>
                <li>Browse our extensive product catalog</li>
                <li>Track your orders in real-time</li>
                <li>Save your favorite items</li>
                <li>Get exclusive deals and offers</li>
            </ul>

            <p>Ready to start shopping?</p>
            <a href='http://localhost:3000' class='button'>Start Shopping</a>

            <p>If you have any questions, feel free to reach out to our support team.</p>
            
            <p>Best regards,<br/>The E-Commerce Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 E-Commerce Platform. All rights reserved.</p>
            <p>This email was sent because you registered for an account.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetLoginNotificationTemplate(string firstName, DateTime loginTime)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .info-box {{ background: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Login Detected</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName}!</h2>
            <p>We detected a new login to your account.</p>
            
            <div class='info-box'>
                <strong>Login Time:</strong> {loginTime:MMMM dd, yyyy 'at' HH:mm:ss}<br/>
                <strong>Account:</strong> Secure
            </div>

            <p>If this was you, no action is needed. Welcome back!</p>
            <p>If you didn't log in, please contact our security team immediately.</p>
            
            <p>Best regards,<br/>The E-Commerce Security Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 E-Commerce Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
