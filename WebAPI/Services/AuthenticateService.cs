using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using Domain;
using WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Services
{
    /// <summary>
    /// Service for handling user authentication
    /// </summary>
    public class AuthenticateService : IAuthenticateService
    {
        private readonly HUDBContext _context;
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticateService> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthenticateService
        /// </summary>
        /// <param name="appSettings">Application settings for JWT configuration</param>
        /// <param name="config">Application configuration</param>
        /// <param name="context">Database context</param>
        /// <param name="logger">Logger instance</param>
        public AuthenticateService(
            IOptions<AppSettings> appSettings,
            IConfiguration config,
            HUDBContext context,
            ILogger<AuthenticateService> logger)
        {
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously authenticates a user using a token or email/password combination
        /// </summary>
        /// <param name="authToken">Authentication token (optional)</param>
        /// <param name="email">User email (optional if authToken is provided)</param>
        /// <param name="password">User password (optional if authToken is provided)</param>
        /// <returns>Authenticated user with token if successful, null otherwise</returns>
        public async Task<User> AuthenticateAsync(string authToken, string email, string password)
        {
            try
            {
                // Token-based authentication
                if (!string.IsNullOrEmpty(authToken))
                {
                    return await ValidateTokenAsync(authToken);
                }

                // Email/password authentication
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    return await AuthenticateWithCredentialsAsync(email, password);
                }

                // No valid authentication method provided
                _logger.LogWarning("Authentication attempt with insufficient credentials");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication");
                return null;
            }
        }

        /// <summary>
        /// Legacy synchronous authentication method
        /// </summary>
        /// <remarks>
        /// This method is kept for backward compatibility.
        /// New implementations should use AuthenticateAsync instead.
        /// </remarks>
        public User Authenticate(string authToken, string email, string password)
        {
            try
            {
                // Token-based authentication
                if (!string.IsNullOrEmpty(authToken))
                {
                    var userId = ValidateTokenAndGetUserId(authToken);
                    if (userId != null)
                    {
                        return _context.User.FirstOrDefault(x => x.UserId == userId);
                    }
                }

                // Email/password authentication
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    var user = _context.User.FirstOrDefault(x => x.Email == email && x.Password == password);
                    if (user != null)
                    {
                        user.Token = GenerateJwtToken(user);

                        var profile = _context.Profile.FirstOrDefault(x => x.UserId == user.UserId);

                        user.Profile = profile;

                        return user;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during synchronous authentication");
                return null;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validates a JWT token and returns the associated user
        /// </summary>
        private async Task<User> ValidateTokenAsync(string token)
        {
            var userId = ValidateTokenAndGetUserId(token);
            if (userId == null)
            {
                return null;
            }

            return await _context.User
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        private async Task<User> AuthenticateWithCredentialsAsync(string email, string password)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(x => x.Email == email && x.Password == password);

            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", email);
                return null;
            }

            // Update login timestamp if needed
            // await UpdateLastLoginAsync(user);

            // Generate JWT token
            user.Token = GenerateJwtToken(user);
            return user;
        }

        /// <summary>
        /// Validates a JWT token and extracts the user ID
        /// </summary>
        private string ValidateTokenAndGetUserId(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Key);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer"),
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                {
                    _logger.LogWarning("Invalid token algorithm");
                    return null;
                }

                // Extract user ID from token claims
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);
                return userIdClaim?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return null;
            }
        }

        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.AccessLevel ?? "Standard")
                }),
                Expires = DateTime.UtcNow.AddDays(30), // 30-day token
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
}