using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Identity.API.Infrastructure.Services
{
    /// <summary>
    /// Redis-based token blacklist service
    /// </summary>
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<TokenBlacklistService> _logger;

        public TokenBlacklistService(ICacheService cacheService, ILogger<TokenBlacklistService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task BlacklistTokenAsync(string token, TimeSpan? expiration = null)
        {
            try
            {
                // Hash the token for privacy and consistent key length
                var tokenHash = ComputeHash(token);
                var key = $"blacklist:token:{tokenHash}";

                // Default expiration: 7 days (max refresh token lifetime)
                var expiryTime = expiration ?? TimeSpan.FromDays(7);

                await _cacheService.SetAsync(key, true, expiryTime);
                
                _logger.LogInformation("Token blacklisted successfully. Expires in: {Expiration}", expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting token");
                throw;
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var tokenHash = ComputeHash(token);
                var key = $"blacklist:token:{tokenHash}";

                var isBlacklisted = await _cacheService.ExistsAsync(key);
                
                if (isBlacklisted)
                {
                    _logger.LogWarning("Attempt to use blacklisted token detected");
                }

                return isBlacklisted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token blacklist");
                // Fail open: if Redis is down, don't block all requests
                return false;
            }
        }

        public async Task BlacklistUserTokensAsync(Guid userId)
        {
            try
            {
                var key = $"blacklist:user:{userId}";
                
                // Mark all tokens for this user as invalid
                await _cacheService.SetAsync(key, true, TimeSpan.FromDays(7));
                
                _logger.LogInformation("All tokens blacklisted for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting user tokens for user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Compute SHA256 hash of token for storage
        /// </summary>
        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
