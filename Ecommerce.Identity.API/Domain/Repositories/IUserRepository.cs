using Ecommerce.Shared.Common;

namespace Ecommerce.Identity.API.Domain.Repositories
{
    /// <summary>
    /// Repository interface for User entity operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their unique identifier
        /// </summary>
        Task<User?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        Task<User?> GetByEmailAsync(string email);
        
        /// <summary>
        /// Gets a user by their refresh token
        /// </summary>
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        
        /// <summary>
        /// Creates a new user
        /// </summary>
        Task<User> CreateAsync(User user);
        
        /// <summary>
        /// Updates an existing user
        /// </summary>
        Task<User> UpdateAsync(User user);
        
        /// <summary>
        /// Checks if a user exists with the given email
        /// </summary>
        Task<bool> ExistsAsync(string email);
    }
}
