using Ecommerce.Identity.API.Application.Interfaces;
using Ecommerce.Identity.API.Domain.Repositories;
using Ecommerce.Identity.API.Infrastructure.Services;
using Ecommerce.Shared.Common;
using Ecommerce.Shared.Common.Constants;
using Ecommerce.Shared.Common.DTOs.Auth;
using Ecommerce.Shared.Common.Events;
using Ecommerce.Shared.Common.Exceptions;
using Ecommerce.Shared.Common.Messaging;
using BCrypt.Net;

namespace Ecommerce.Identity.API.Application.Services
{
    /// <summary>
    /// Service implementation for authentication operations
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ICacheService _cacheService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            ICacheService cacheService,
            IEventPublisher eventPublisher,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _cacheService = cacheService;
            _eventPublisher = eventPublisher;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Registering new user with email: {Email}", request.Email);

            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Email))
            {
                _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
                throw new ConflictException($"User with email {request.Email} already exists");
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Roles = new List<string> { RoleConstants.Customer }, // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7"));

            // Save user to database
            await _userRepository.CreateAsync(user);

            _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);

            // ========== PHASE 3 - DAY 2: Publish UserRegistered Event ==========
            try
            {
                var userRegisteredEvent = new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RegisteredAt = user.CreatedAt,
                    Roles = user.Roles.ToArray(),
                    CorrelationId = Guid.NewGuid().ToString()
                };

                await _eventPublisher.PublishAsync(userRegisteredEvent, "userregisteredevent");
                _logger.LogInformation("Published UserRegistered event for user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserRegistered event for user: {UserId}", user.Id);
                // Don't throw - registration succeeded even if event publishing failed
            }
            // ========== END PHASE 3 - DAY 2 ==========

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationInMinutes"] ?? "60") * 60,
                User = MapToUserDTO(user)
            };
        }

        /// <summary>
        /// Authenticates a user and returns tokens
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Try to get user from cache first
            var cacheKey = $"user:email:{request.Email}";
            var user = await _cacheService.GetAsync<User>(cacheKey);

            if (user != null)
            {
                _logger.LogDebug("User found in cache for email: {Email}", request.Email);
            }
            else
            {
                // Cache miss - fetch from database
                user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
                    throw new UnauthorizedException("Invalid email or password");
                }

                // Cache the user for 1 hour
                await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
                _logger.LogDebug("User cached for email: {Email}", request.Email);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {UserId} is not active", user.Id);
                throw new UnauthorizedException("Account is inactive");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                throw new UnauthorizedException("Invalid email or password");
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7"));
            user.LastLoginAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Update cache with new data
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            // ========== PHASE 3 - DAY 2: Publish UserLoggedIn Event ==========
            try
            {
                var userLoggedInEvent = new UserLoggedInEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    LoginAt = user.LastLoginAt ?? DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid().ToString()
                    // IpAddress and UserAgent can be added from HttpContext in controller
                };

                await _eventPublisher.PublishAsync(userLoggedInEvent, "userlogginedevent");
                _logger.LogInformation("Published UserLoggedIn event for user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserLoggedIn event for user: {UserId}", user.Id);
                // Don't throw - login succeeded even if event publishing failed
            }
            // ========== END PHASE 3 - DAY 2 ==========

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationInMinutes"] ?? "60") * 60,
                User = MapToUserDTO(user)
            };
        }

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh token request received");

            // Validate the access token (without lifetime validation)
            var principal = _jwtService.ValidateToken(request.AccessToken);
            
            if (principal == null)
            {
                _logger.LogWarning("Refresh token failed: Invalid access token");
                throw new UnauthorizedException("Invalid access token");
            }

            // Get user ID from token claims
            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Refresh token failed: Invalid user ID in token");
                throw new UnauthorizedException("Invalid token claims");
            }

            // Find user by refresh token
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
            
            if (user == null || user.Id != userId)
            {
                _logger.LogWarning("Refresh token failed: Invalid refresh token for user {UserId}", userId);
                throw new UnauthorizedException("Invalid refresh token");
            }

            // Check if refresh token has expired
            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token failed: Refresh token expired for user {UserId}", userId);
                throw new UnauthorizedException("Refresh token has expired");
            }

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7"));

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpirationInMinutes"] ?? "60") * 60,
                User = MapToUserDTO(user)
            };
        }

        /// <summary>
        /// Gets the current user profile
        /// </summary>
        public async Task<UserDTO> GetProfileAsync(Guid userId)
        {
            _logger.LogInformation("Getting profile for user: {UserId}", userId);

            // Try cache first
            var cacheKey = $"user:id:{userId}";
            var user = await _cacheService.GetAsync<User>(cacheKey);

            if (user != null)
            {
                _logger.LogDebug("User profile found in cache: {UserId}", userId);
            }
            else
            {
                // Cache miss - fetch from database
                user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("Profile not found for user: {UserId}", userId);
                    throw new NotFoundException($"User with ID {userId} not found");
                }

                // Cache for 1 hour
                await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
                _logger.LogDebug("User profile cached: {UserId}", userId);
            }

            return MapToUserDTO(user);
        }

        /// <summary>
        /// Logs out a user by blacklisting their token
        /// </summary>
        public async Task LogoutAsync(Guid userId, string accessToken)
        {
            _logger.LogInformation("Logout request for user: {UserId}", userId);

            // Invalidate the access token
            var tokenExpiration = TimeSpan.FromMinutes(
                Convert.ToInt32(_configuration["Jwt:ExpirationInMinutes"] ?? "60"));

            var tokenBlacklistKey = $"blacklist:token:{ComputeTokenHash(accessToken)}";
            await _cacheService.SetAsync(tokenBlacklistKey, true, tokenExpiration);

            // Clear user cache
            var userCacheKey = $"user:id:{userId}";
            await _cacheService.RemoveAsync(userCacheKey);

            // Also clear email-based cache if exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                var emailCacheKey = $"user:email:{user.Email}";
                await _cacheService.RemoveAsync(emailCacheKey);
            }

            _logger.LogInformation("User logged out successfully: {UserId}", userId);
        }

        /// <summary>
        /// Compute hash of token for blacklist storage
        /// </summary>
        private static string ComputeTokenHash(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Maps User entity to UserDTO
        /// </summary>
        private static UserDTO MapToUserDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
