using Ecommerce.Shared.Common;
using System.Security.Claims;

namespace Ecommerce.Identity.API.Application.Interfaces
{
    /// <summary>
    /// Service interface for JWT token operations
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT access token for the user
        /// </summary>
        string GenerateAccessToken(User user);
        
        /// <summary>
        /// Generates a refresh token
        /// </summary>
        string GenerateRefreshToken();
        
        /// <summary>
        /// Validates a JWT token and returns the principal
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);
    }
}
