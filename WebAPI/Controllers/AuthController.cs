using Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for user authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthenticationService authService,
            IUserService userService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful
        /// </summary>
        /// <param name="request">The authentication request</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResultDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 401)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            try
            {
                var result = await _authService.AuthenticateAsync(request.Email, request.Password);

                if (result == null)
                {
                    _logger.LogWarning("Authentication failed for email: {Email}", request.Email);
                    return Unauthorized(new ErrorDto { Message = "Invalid email or password" });
                }

                // Update last login date
                await _userService.UpdateLastLoginDateAsync(result.UserId);

                _logger.LogInformation("User authenticated successfully: {UserId}", result.UserId);

                return Ok(new AuthResultDto
                {
                    UserId = result.UserId,
                    Token = result.Token,
                    AccessLevel = result.AccessLevel,
                    ExpiresAt = result.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication error");
                return StatusCode(500, new ErrorDto { Message = "An error occurred during authentication" });
            }
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">The registration request</param>
        /// <returns>Registration result</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResultDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid registration data",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            try
            {
                // Check if email already exists
                var emailExists = await _userService.IsEmailAvailableAsync(request.Email);
                if (emailExists)
                {
                    return Conflict(new ErrorDto { Message = "Email is already in use" });
                }

                // Check if username is available
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    var usernameExists = await _userService.IsUserNameAvailableAsync(request.UserName);
                    if (usernameExists)
                    {
                        return Conflict(new ErrorDto { Message = "Username is already in use" });
                    }
                }

                // Create user
                var user = new User
                {
                    Email = request.Email,
                    Password = request.Password, // Will be hashed by the service
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    SignUpDate = DateTime.UtcNow.ToString("o"),
                    AccessLevel = "Standard",
                    Status = "Active"
                };

                // Create profile for the user
                var profile = new Profile
                {
                    UserName = request.UserName ?? $"{request.FirstName}{request.LastName}".ToLower(),
                    StarRating = "1",
                    Ranking = "1",
                    Points = 2
                };

                user.Profile = profile;

                var userId = await _userService.CreateUserAsync(user);
                if (string.IsNullOrEmpty(userId))
                {
                    return StatusCode(500, new ErrorDto { Message = "Failed to create user" });
                }

                // Authenticate the new user
                var authResult = await _authService.AuthenticateAsync(request.Email, request.Password);

                _logger.LogInformation("User registered successfully: {UserId}", userId);

                return CreatedAtAction(nameof(Login), new AuthResultDto
                {
                    UserId = authResult.UserId,
                    Token = authResult.Token,
                    AccessLevel = authResult.AccessLevel,
                    ExpiresAt = authResult.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, new ErrorDto { Message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Request a password reset
        /// </summary>
        /// <param name="request">The password reset request</param>
        /// <returns>Success message</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            try
            {
                // Check if user with email exists
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return NotFound(new ErrorDto { Message = "User not found" });
                }

                // Generate password reset token
                await _userService.GeneratePasswordResetTokenAsync(user.UserId);

                _logger.LogInformation("Password reset requested for user: {UserId}", user.UserId);

                // Don't reveal if user exists in production for security
                return Ok(new MessageDto { Message = "If your email is registered, you will receive a password reset link" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset request error");
                return StatusCode(500, new ErrorDto { Message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        /// <param name="request">The reset password request</param>
        /// <returns>Success message</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            try
            {
                var result = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
                if (!result)
                {
                    return BadRequest(new ErrorDto { Message = "Invalid or expired token" });
                }

                return Ok(new MessageDto { Message = "Password has been reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset error");
                return StatusCode(500, new ErrorDto { Message = "An error occurred while resetting your password" });
            }
        }
    }
}