using Ecommerce.Identity.API.Application.Interfaces;
using Ecommerce.Shared.Common.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.Identity.API.Controllers
{
    /// <summary>
    /// Controller for authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration request containing user details</param>
        /// <returns>Login response with tokens and user information</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", request.Email);
            
            var response = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, response);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login request containing credentials</param>
        /// <returns>Login response with tokens and user information</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login endpoint called for email: {Email}", request.Email);
            
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New tokens</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh token endpoint called");
            
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>User profile information</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Invalid user ID in token claims");
                return Unauthorized("Invalid token claims");
            }

            _logger.LogInformation("Get profile endpoint called for user: {UserId}", userId);

            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// Logout and invalidate current access token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Invalid user ID in token claims during logout");
                return Unauthorized("Invalid token claims");
            }

            // Get the token from the Authorization header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            _logger.LogInformation("Logout endpoint called for user: {UserId}", userId);

            await _authService.LogoutAsync(userId, token);

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
