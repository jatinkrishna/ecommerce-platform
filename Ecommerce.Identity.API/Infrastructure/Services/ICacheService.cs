namespace Ecommerce.Identity.API.Infrastructure.Services
{
    /// <summary>
    /// Interface for cache operations
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get cached value by key
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Set cache value with optional expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Remove cached value by key
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Check Redis health status
        /// </summary>
        Task<bool> IsHealthyAsync();
    }
}
