namespace Ecommerce.Identity.API.Infrastructure.Services
{
    /// <summary>
    /// Service for managing blacklisted JWT tokens
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Add token to blacklist
        /// </summary>
        Task BlacklistTokenAsync(string token, TimeSpan? expiration = null);

        /// <summary>
        /// Check if token is blacklisted
        /// </summary>
        Task<bool> IsTokenBlacklistedAsync(string token);

        /// <summary>
        /// Blacklist all tokens for a user
        /// </summary>
        Task BlacklistUserTokensAsync(Guid userId);
    }
}
