using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAPI.DTOs;
using WebAPI.Services;
using DataLayer.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticateService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;

        public AuthController(
            IAuthenticateService authService,
            IUserRepository userRepository,
            IConfiguration configuration,
            IPasswordService passwordService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Authenticate user
                var user = await _authService.Authenticate(null, request.Email, request.Password);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Return token info
                return Ok(new AuthResultDto
                {
                    UserId = user.UserId,
                    Token = user.Token,
                    AccessLevel = user.AccessLevel,
                    ExpiresAt = GetExpirationDate()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if email is available
                if (!await _userRepository.IsEmailAvailableAsync(request.Email))
                {
                    return BadRequest(new { message = "Email is already in use" });
                }

                // Create new user with current date
                DateTime now = DateTime.Now;
                var user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    PasswordHash = _authService.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    // According to Domain/User.cs, the property is actually named "SignUpDate" 
                    // and it's a string type
                    SignUpDate = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    AccessLevel = "Standard",
                    Status = "Active"
                };

                // Create associated profile
                var profile = new Profile
                {
                    ProfileId = Guid.NewGuid().ToString(),
                    UserId = user.UserId,
                    UserName = request.UserName ?? $"{request.FirstName}{request.LastName}",
                    ImageURL = $"https://uhblobstorageaccount.blob.core.windows.net/profileimage/{user.UserId}.webp"
                };

                user.Profile = profile;

                // Save user to database
                await _userRepository.AddAsync(user);
                await _userRepository.SaveAsync();

                // Generate auth token
                user.Token = GenerateJwtToken(user);

                // Return token info
                return Ok(new AuthResultDto
                {
                    UserId = user.UserId,
                    Token = user.Token,
                    AccessLevel = user.AccessLevel,
                    ExpiresAt = GetExpirationDate()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Find user by email
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return Ok(new { message = "If your email is registered, you will receive a password reset link" });
                }

                // Generate reset token
                var resetToken = Guid.NewGuid().ToString();
                var resetLink = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password?token={resetToken}";

                // Save reset token to user
                user.ResetCode = resetToken;
                user.ResetLink = resetLink;

                _userRepository.Update(user);
                await _userRepository.SaveAsync();

                // TODO: Send email with reset link
                // For now, just return the link
                return Ok(new { message = "Password reset link has been sent", resetLink });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Find user by reset token
                var user = await _userRepository.FindAsync(u => u.ResetCode == request.Token);
                if (user == null || !user.Any())
                {
                    return BadRequest(new { message = "Invalid or expired token" });
                }

                var foundUser = user.First();

                // Update password
                foundUser.PasswordHash = _authService.HashPassword(request.NewPassword);
                foundUser.ResetCode = null;
                foundUser.ResetLink = null;

                _userRepository.Update(foundUser);
                await _userRepository.SaveAsync();

                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Don't return sensitive information
                return Ok(new
                {
                    user.UserId,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.AccessLevel,
                    Profile = user.Profile == null ? null : new
                    {
                        user.Profile.ProfileId,
                        user.Profile.UserName,
                        user.Profile.ImageURL
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        #region Helper Methods

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("UserId", user.UserId),
                new Claim(ClaimTypes.Role, user.AccessLevel ?? "Standard"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add profile ID claim if available
            if (user.Profile != null && !string.IsNullOrEmpty(user.Profile.ProfileId))
            {
                claims.Add(new Claim("ProfileId", user.Profile.ProfileId));
            }

            // Set token expiration from configuration or default to 1 day
            var expirationHours = 24;
            if (int.TryParse(_configuration["Jwt:ExpirationHours"], out int configHours))
            {
                expirationHours = configHours;
            }

            // Create token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private DateTime? GetExpirationDate()
        {
            var expirationHours = 24;
            if (int.TryParse(_configuration["Jwt:ExpirationHours"], out int configHours))
            {
                expirationHours = configHours;
            }

            return DateTime.UtcNow.AddHours(expirationHours);
        }

        #endregion
    }
}