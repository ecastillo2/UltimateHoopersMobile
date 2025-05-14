// Services/AuthService.cs
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
        private const string USER_ID_KEY = "user_id";

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
                    // Store the token and user ID in secure storage
                    await SecureStorage.SetAsync(TOKEN_KEY, user.Token);
                    await SecureStorage.SetAsync(USER_ID_KEY, user.UserId);

                    // Update the global token for easy access
                    App.AuthToken = user.Token;

                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                throw new Exception("Login failed. Please check your credentials and try again.", ex);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Clear the stored token and user ID
                await SecureStorage.SetAsync(TOKEN_KEY, string.Empty);
                await SecureStorage.SetAsync(USER_ID_KEY, string.Empty);

                // Clear the global token
                App.AuthToken = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(TOKEN_KEY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving auth token");
                return null;
            }
        }
    }
}