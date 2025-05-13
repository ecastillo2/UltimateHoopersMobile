// File: WebAPI/Services/AuthenticationService.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DataLayer.DAL;
using Domain;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Generators;
using System.Text.RegularExpressions;

namespace WebAPI.Services
{
    /// <summary>
    /// Service to handle user authentication and token generation
    /// </summary>
    public class AuthenticationService : IAuthenticateService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticate user using email and password
        /// </summary>
        /// <param name="token">External auth token (if applicable)</param>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <returns>Authenticated user with token or null if authentication fails</returns>
        public async Task<User> Authenticate(string token, string email, string password)
        {
            try
            {
                // First, try to authenticate using external token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    return await AuthenticateWithToken(token);
                }

                // If no token, use email/password
                return await AuthenticateWithCredentials(email, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for email: {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Authenticate using external token (e.g., OAuth)
        /// </summary>
        private async Task<User> AuthenticateWithToken(string token)
        {
            try
            {
                // Validate token (implement validation logic here)
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Extract user email from token claims
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    _logger.LogWarning("No email claim found in token");
                    return null;
                }

                // Get user by email
                var user = await _userRepository.GetUserByEmail(emailClaim);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", emailClaim);
                    return null;
                }

                // Generate new JWT token for our system
                user.Token = GenerateJwtToken(user);

                // Update last login date
                await _userRepository.UpdateLastLoginDate(user.UserId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate with token");
                return null;
            }
        }

        /// <summary>
        /// Authenticate using email and password
        /// </summary>
        private async Task<User> AuthenticateWithCredentials(string email, string password)
        {
            // Return null if email or password is empty
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Authentication attempt with empty email or password");
                return null;
            }

            // Get user by email
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return null;
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for email: {Email}", email);
                return null;
            }

            // Update user token
            user.Token = GenerateJwtToken(user);

            // Update last login date
            await _userRepository.UpdateLastLoginDate(user.UserId);

            return user;
        }

        /// <summary>
        /// Generate JWT token for user
        /// </summary>
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

        /// <summary>
        /// Verify password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="passwordHash">Stored password hash</param>
        /// <returns>True if password matches hash</returns>
        private bool VerifyPassword(string password, string passwordHash)
        {
            // Use BCrypt to verify password
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        /// <summary>
        /// Hash password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password)
        {
            // Use BCrypt to hash password with work factor of 12
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }
    }

    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthenticateService
    {
        Task<User> Authenticate(string token, string email, string password);
    }
}

