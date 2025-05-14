using ApiClient.Authentication;
using Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace UltimateHoopers.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthenticateUser _authenticateUser;
        private readonly ILogger<AuthService> _logger;
        private const string TOKEN_KEY = "auth_token";
        private const string TOKEN_EXPIRATION_KEY = "token_expiration";
        private const string USER_ID_KEY = "user_id";
        private const string EMAIL_KEY = "user_email";
        private const string PASSWORD_KEY = "user_password"; // Note: storing password is risky, consider alternatives

        public AuthService(IAuthenticateUser authenticateUser, ILogger<AuthService> logger)
        {
            _authenticateUser = authenticateUser ?? throw new ArgumentNullException(nameof(authenticateUser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _authenticateUser.AuthenticateAsync(email, password);

                if (user != null && !string.IsNullOrEmpty(user.Token))
                {
                    // Store the token and user details in secure storage
                    await SecureStorage.SetAsync(TOKEN_KEY, user.Token);
                    await SecureStorage.SetAsync(USER_ID_KEY, user.UserId);
                    await SecureStorage.SetAsync(EMAIL_KEY, email);

                    // Store token expiration if available
                    if (user.TokenExpiration.HasValue)
                    {
                        await SecureStorage.SetAsync(TOKEN_EXPIRATION_KEY,
                            user.TokenExpiration.Value.ToString("o")); // ISO 8601 format
                    }

                    // Update the global token for easy access
                    App.AuthToken = user.Token;

                    return user;
                }

                throw new AuthenticationException("Login failed: Invalid response from server");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Clear all stored authentication data
                var keysToRemove = new[] { TOKEN_KEY, USER_ID_KEY, EMAIL_KEY, TOKEN_EXPIRATION_KEY, PASSWORD_KEY };

                foreach (var key in keysToRemove)
                {
                    try
                    {
                        await SecureStorage.Default.SetAsync(key, string.Empty);
                    }
                    catch (Exception keyEx)
                    {
                        // Log but continue with other keys
                        _logger.LogWarning(keyEx, $"Could not clear {key} from secure storage");
                    }
                }

                // Clear the global token
                App.AuthToken = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Don't rethrow to ensure logout completes even if there are errors
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return false;

                // Check if token is expired
                var expirationString = await SecureStorage.GetAsync(TOKEN_EXPIRATION_KEY);
                if (!string.IsNullOrEmpty(expirationString) &&
                    DateTime.TryParse(expirationString, out DateTime expiration))
                {
                    // Add a buffer of 5 minutes to account for network latency
                    if (expiration <= DateTime.UtcNow.AddMinutes(5))
                    {
                        _logger.LogInformation("Token is expired or about to expire. Refreshing...");
                        return await RefreshTokenAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(TOKEN_KEY);

                // If token exists but might be expired, check and refresh if needed
                if (!string.IsNullOrEmpty(token))
                {
                    var expirationString = await SecureStorage.GetAsync(TOKEN_EXPIRATION_KEY);
                    if (!string.IsNullOrEmpty(expirationString) &&
                        DateTime.TryParse(expirationString, out DateTime expiration))
                    {
                        // Add a buffer of 5 minutes
                        if (expiration <= DateTime.UtcNow.AddMinutes(5))
                        {
                            await RefreshTokenAsync();
                            // Get the new token
                            token = await SecureStorage.GetAsync(TOKEN_KEY);
                        }
                    }
                }

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving auth token");
                return null;
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            try
            {
                // Get stored credentials
                var email = await SecureStorage.GetAsync(EMAIL_KEY);
                var password = await SecureStorage.GetAsync(PASSWORD_KEY);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Cannot refresh token: Missing credentials");
                    return false;
                }

                // Re-authenticate to get a new token
                var user = await _authenticateUser.AuthenticateAsync(email, password);

                if (user != null && !string.IsNullOrEmpty(user.Token))
                {
                    // Update stored token and expiration
                    await SecureStorage.SetAsync(TOKEN_KEY, user.Token);

                    if (user.TokenExpiration.HasValue)
                    {
                        await SecureStorage.SetAsync(TOKEN_EXPIRATION_KEY,
                            user.TokenExpiration.Value.ToString("o"));
                    }

                    // Update global token
                    App.AuthToken = user.Token;

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return false;
            }
        }
    }
}