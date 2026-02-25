using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.Identity.API.Infrastructure.Services
{
    /// <summary>
    /// Redis cache service implementation
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly int _defaultExpiration;

        public RedisCacheService(
            IDistributedCache cache,
            ILogger<RedisCacheService> logger,
            IConfiguration configuration)
        {
            _cache = cache;
            _logger = logger;
            _defaultExpiration = configuration.GetValue<int>("Redis:DefaultExpiration", 3600);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromSeconds(_defaultExpiration)
                };

                await _cache.SetStringAsync(key, serializedValue, options);
                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
                    key, expiration ?? TimeSpan.FromSeconds(_defaultExpiration));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("Removed cache for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var testKey = "health_check_test";
                var testValue = "healthy";
                
                // Try to write
                await SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                
                // Try to read
                var result = await GetAsync<string>(testKey);
                
                // Clean up
                await RemoveAsync(testKey);
                
                var isHealthy = result == testValue;
                
                if (isHealthy)
                {
                    _logger.LogInformation("Redis health check passed");
                }
                else
                {
                    _logger.LogWarning("Redis health check failed: value mismatch");
                }
                
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check failed with exception");
                return false;
            }
        }
    }
}
