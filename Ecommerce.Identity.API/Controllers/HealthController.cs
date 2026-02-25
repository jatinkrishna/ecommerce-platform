using Ecommerce.Identity.API.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Identity.API.Controllers
{
    /// <summary>
    /// Health check endpoints for monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ICacheService cacheService, ILogger<HealthController> logger)
        {
            _cacheService = cacheService;
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
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Identity API",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// Redis health check
        /// </summary>
        [HttpGet("redis")]
        public async Task<IActionResult> CheckRedis()
        {
            try
            {
                var isHealthy = await _cacheService.IsHealthyAsync();
                
                if (isHealthy)
                {
                    _logger.LogInformation("Redis health check successful");
                    return Ok(new
                    {
                        status = "healthy",
                        service = "Redis",
                        timestamp = DateTime.UtcNow,
                        message = "Redis is responding correctly"
                    });
                }
                
                _logger.LogWarning("Redis health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    service = "Redis",
                    timestamp = DateTime.UtcNow,
                    message = "Redis is not responding"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check threw exception");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    service = "Redis",
                    timestamp = DateTime.UtcNow,
                    message = "Redis health check failed with exception",
                    error = ex.Message
                });
            }
        }
    }
}
