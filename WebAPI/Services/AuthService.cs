using DataLayer.DAL;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    /// <summary>
    /// Service for authenticating users and generating JWT tokens
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IProfileRepository profileRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticate a user using token or email/password combination
        /// </summary>
        public async Task<User> Authenticate(string token, string email, string password)
        {
            try
            {
                // Try to authenticate using token
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("Attempting to authenticate using token");
                    return await AuthenticateWithToken(token);
                }

                // Try to authenticate using email and password
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    _logger.LogInformation("Attempting to authenticate using email and password");
                    return await AuthenticateWithCredentials(email, password);
                }

                // If no valid authentication method was provided
                _logger.LogWarning("No valid authentication method provided");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication");
                return null;
            }
        }

        private async Task<User> AuthenticateWithToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                // Validate token
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal;
                try
                {
                    principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Token validation failed");
                    return null;
                }

                // Extract user ID from claims
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogWarning("Token does not contain user ID claim");
                    return null;
                }

                // Get user from database
                var userId = userIdClaim.Value;
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} from token not found", userId);
                    return null;
                }

                if (user.Status != "Active")
                {
                    _logger.LogWarning("User account with ID {UserId} is not active", userId);
                    return null;
                }

                // Update last login
                await _userRepository.UpdateLastLoginDateAsync(userId);

                // Generate a new token
                user.Token = GenerateJwtToken(user);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with token");
                return null;
            }
        }

        private async Task<User> AuthenticateWithCredentials(string email, string password)
        {
            try
            {
                // Get user by email
                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("User with email {Email} not found", email);
                    return null;
                }

                // Verify password
                if (!_userRepository.VerifyPassword(user, password))
                {
                    _logger.LogWarning("Invalid password for user with email {Email}", email);
                    return null;
                }

                if (user.Status != "Active")
                {
                    _logger.LogWarning("User account with email {Email} is not active", email);
                    return null;
                }

                // Update last login
                await _userRepository.UpdateLastLoginDateAsync(user.UserId);

                // Generate JWT token
                user.Token = GenerateJwtToken(user);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with credentials");
                return null;
            }
        }

        private string GenerateJwtToken(User user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                // Get token expiration from configuration (default to 24 hours)
                double expirationHours;
                if (!double.TryParse(_configuration["Jwt:ExpirationHours"], out expirationHours))
                {
                    expirationHours = 24;
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.AccessLevel ?? "Standard"),
                        new Claim("ProfileId", user.ProfileId ?? string.Empty)
                    }),
                    Expires = DateTime.UtcNow.AddHours(expirationHours),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"]
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.UserId);
                throw;
            }
        }
    }
}