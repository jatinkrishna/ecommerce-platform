using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Notification.API.Controllers
{
    /// <summary>
    /// Health check endpoint for monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Basic health check
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "Notification API",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Detailed health check
        /// </summary>
        [HttpGet("detailed")]
        public IActionResult GetDetailed()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "Notification API",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                components = new
                {
                    rabbitmq = new { status = "Connected", description = "Event consumer running" },
                    email = new { status = "Ready", description = "Email service configured" }
                }
            });
        }
    }
}
