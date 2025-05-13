using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Domain;
using DataLayer;
using WebAPI.Models;

namespace WebAPI.Services
{
    /// <summary>
    /// Authenticate Service
    /// </summary>
    public class AuthenticateService : IAuthenticateService
    {
        private readonly HUDBContext _context;
        private readonly AppSettings _appSettings;
        private IConfiguration _config;
        /// <summary>
        /// AuthenticateService
        /// </summary>
        /// <param name="appSettings"></param>
        public AuthenticateService(IOptions<AppSettings> appSettings, IConfiguration config, HUDBContext context)
        {
            _appSettings = appSettings.Value;
            _config = config;
            _context = context;
        }


        /// <summary>
        /// SocialMediaAuthenticate
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User? SocialMediaAuthenticate(string? authToken, string? email = null, string? password = null)
        {
            using (_context)
            {
                if (!string.IsNullOrEmpty(authToken))
                {
                    // Validate JWT Token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Key);

                    try
                    {
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

                        // DEBUG: Log the token being validated
                        Console.WriteLine($"Validating Token: {authToken}");

                        var principal = tokenHandler.ValidateToken(authToken, tokenValidationParameters, out SecurityToken validatedToken);

                        if (validatedToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                        {
                            Console.WriteLine("Invalid token algorithm.");
                            return null;
                        }


                        // Use "sub" or "NameIdentifier" as the claim type
                        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);
                        if (userIdClaim != null)
                        {
                            var userId = userIdClaim.Value;
                            
                            return _context.User.FirstOrDefault(x => x.UserId == userId );
                        }
                    }
                    catch (Exception)
                    {
                        return null; // Invalid token
                    }
                }

                // If `AuthToken` is null, authenticate via email and password
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    var user = _context.User.FirstOrDefault(x => x.Email == email && x.Password == password);
                    if (user == null)
                    {
                        return null; // Authentication failed
                    }

                    var key = Encoding.ASCII.GetBytes(_appSettings.Key);
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Role, user.AccessLevel)
                }),
                        Expires = DateTime.UtcNow.AddMonths(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = _config["Jwt:Issuer"],
                        Audience = _config["Jwt:Audience"]
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    user.AuthToken = tokenHandler.WriteToken(token);

                    return user;
                }

                return null;
            }
        }

    }

}

