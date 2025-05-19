using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiClient.Authentication
{
    public class AuthenticateUser : IAuthenticateUser
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticateUser> _logger;
        private readonly string _baseUrl;

        public AuthenticateUser(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AuthenticateUser> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get base URL from configuration
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _baseUrl = "https://ultimatehoopersapi.azurewebsites.net";
                _logger.LogWarning("API base URL not found in configuration. Using default: {BaseUrl}", _baseUrl);
            }
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

                    // Return more info about the failure
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    throw new AuthenticationException($"Authentication failed: {response.StatusCode}",
                        errorResponse, (int)response.StatusCode);
                }

                // Read and parse the response
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Deserialize the authentication result
                User user = JsonSerializer.Deserialize<User>(responseContent, options);

                if (user == null)
                {
                    throw new AuthenticationException("Failed to deserialize authentication response");
                }

                // Check if token is present
                if (string.IsNullOrEmpty(user.Token))
                {
                    throw new AuthenticationException("Authentication response did not contain a token");
                }

              

                return user;
            }
            catch (AuthenticationException)
            {
                // Re-throw authentication exceptions
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during authentication for email: {Email}", email);
                throw new AuthenticationException("Network error occurred during authentication", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email: {Email}", email);
                throw new AuthenticationException("Authentication failed", ex);
            }
        }
    }

    // Custom exception for authentication failures
    public class AuthenticationException : Exception
    {
        public string ResponseContent { get; }
        public int? StatusCode { get; }

        public AuthenticationException(string message) : base(message)
        {
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AuthenticationException(string message, string responseContent, int statusCode = 0) : base(message)
        {
            ResponseContent = responseContent;
            StatusCode = statusCode > 0 ? statusCode : null;
        }
    }

    // AuthResult class moved outside the AuthenticateUser class
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
        public bool? IsHost { get; set; }
        public Profile? Profile { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public AccountType AccountType { get; internal set; }
    }
}