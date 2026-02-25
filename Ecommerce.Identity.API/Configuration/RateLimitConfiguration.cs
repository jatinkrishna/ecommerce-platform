using AspNetCoreRateLimit;

namespace Ecommerce.Identity.API.Configuration
{
    /// <summary>
    /// Rate limiting configuration extension
    /// </summary>
    public static class RateLimitConfiguration
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            // Required to store rate limit counters
            services.AddMemoryCache();

            // Load configuration from appsettings
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            // Inject counter and rules stores
            services.AddInMemoryRateLimiting();

            // Configuration
            services.AddSingleton<IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

            return services;
        }
    }
}
