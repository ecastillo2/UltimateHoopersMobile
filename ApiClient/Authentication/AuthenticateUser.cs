using Domain;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiClient.Authentication
{
    public class AuthenticateUser : IAuthenticateUser
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticateUser> _logger;
        private readonly string _baseUrl;

        // Global static AuthResult property to be accessed throughout the app
        public static AuthResult CurrentAuthResult { get; private set; }

        public AuthenticateUser(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AuthenticateUser> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            try
            {
                // Prepare login request payload
                var loginRequest = new
                {
                    Email = email,
                    Password = password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json");

                // Send login request to the API
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Authentication/Authenticate", content);

                // Check for successful response
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Authentication failed for email: {Email}, Status code: {StatusCode}",
                        email, response.StatusCode);
                    return null;
                }

                // Read and parse the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Store authentication result in static property for global access
                CurrentAuthResult = JsonSerializer.Deserialize<AuthResult>(responseContent, options);

                // Create a user object with the authentication result
                var authenticatedUser = new User
                {
                    UserId = CurrentAuthResult.UserId,
                    ProfileId = CurrentAuthResult.ProfileId,
                    FirstName = CurrentAuthResult.FirstName,
                    LastName = CurrentAuthResult.LastName,
                    SegId = CurrentAuthResult.SegId,
                    SubId = CurrentAuthResult.SubId,
                    Email = email,
                    Token = CurrentAuthResult.Token,
                    AccessLevel = CurrentAuthResult.AccessLevel
                };

                return authenticatedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email: {Email}", email);
                return null;
            }
        }
    }

    // Public AuthResult class moved outside the AuthenticateUser class for global access
    public class AuthResult
    {
        public string? UserId { get; set; }
        public string? ProfileId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? SegId { get; set; }
        public string? SubId { get; set; }
        public string? Token { get; set; }
        public string? AccessLevel { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}