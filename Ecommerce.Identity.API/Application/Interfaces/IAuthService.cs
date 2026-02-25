using Ecommerce.Shared.Common.DTOs.Auth;

namespace Ecommerce.Identity.API.Application.Interfaces
{
    /// <summary>
    /// Service interface for authentication operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user
        /// </summary>
        Task<LoginResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates a user and returns tokens
        /// </summary>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Gets the current user profile
        /// </summary>
        Task<UserDTO> GetProfileAsync(Guid userId);

        /// <summary>
        /// Logs out a user by blacklisting their token
        /// </summary>
        Task LogoutAsync(Guid userId, string accessToken);
    }
}
