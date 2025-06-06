using Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Website.Services
{
    public class AuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthenticationService> _logger;

        // Cookie configuration
        private const string REMEMBER_COOKIE_NAME = "UH_RememberMe";
        private const string USER_DATA_COOKIE_NAME = "UH_UserData";
        private const int REMEMBER_ME_DAYS = 30;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(GetToken());

        public string GetToken()
        {
            // First check session
            var sessionToken = _httpContextAccessor.HttpContext?.Session.GetString("UserToken");
            if (!string.IsNullOrEmpty(sessionToken))
            {
                return sessionToken;
            }

            // If no session token, check remember me cookies
            return GetTokenFromRememberMeCookie();
        }

        public void StoreUserSession(User user, bool rememberMe = false)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null && user != null)
            {
                // Store in session (temporary)
                session.SetString("UserToken", user.Token);
                session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                session.SetString("UserRole", user.AccessLevel);
                session.SetString("UserId", user.UserId);
                session.SetString("ProfileId", user.Profile.ProfileId);

                // Store additional user info as needed
                if (!string.IsNullOrEmpty(user.ClientId))
                    session.SetString("ClientId", user.ClientId);

                // Store login timestamp
                session.SetString("LoginTimestamp", DateTime.UtcNow.ToString("o"));

                _logger.LogInformation("User session stored for user: {UserId}", user.UserId);

                // If remember me is checked, create persistent cookies
                if (rememberMe)
                {
                    CreateRememberMeCookies(user);
                    _logger.LogInformation("Remember me cookies created for user: {UserId}", user.UserId);
                }
            }
        }

        public void ClearUserSession()
        {
            // Clear session
            _httpContextAccessor.HttpContext?.Session.Clear();

            // Clear remember me cookies
            ClearRememberMeCookies();

            _logger.LogInformation("User session and remember me cookies cleared");
        }

        public bool IsUserInRole(string role)
        {
            var userRole = GetUserRole();
            return !string.IsNullOrEmpty(userRole) && userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        public string GetUserRole()
        {
            // First check session
            var sessionRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(sessionRole))
            {
                return sessionRole;
            }

            // If no session, check remember me cookies
            var userData = GetUserDataFromRememberMeCookie();
            return userData?.UserRole;
        }

        public string GetUserId()
        {
            // First check session
            var sessionUserId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId))
            {
                return sessionUserId;
            }

            // If no session, check remember me cookies
            var userData = GetUserDataFromRememberMeCookie();
            return userData?.UserId;
        }

        public string GetUserName()
        {
            // First check session
            var sessionUserName = _httpContextAccessor.HttpContext?.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(sessionUserName))
            {
                return sessionUserName;
            }

            // If no session, check remember me cookies
            var userData = GetUserDataFromRememberMeCookie();
            return userData?.UserName;
        }

        // Updated user type properties
        public bool IsClient => IsUserInRole("Client");
        public bool IsAdmin => IsUserInRole("Admin");

        #region Remember Me Cookie Methods

        private void CreateRememberMeCookies(User user)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                // Create a secure token for the remember me cookie
                var rememberToken = GenerateSecureToken();

                // Create user data object to store in cookie
                var userData = new RememberMeUserData
                {
                    UserId = user.UserId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    UserRole = user.AccessLevel,
                    ProfileId = user.Profile.ProfileId,
                    Token = user.Token,
                    ClientId = user.ClientId,
                    LoginTimestamp = DateTime.UtcNow.ToString("o"),
                    RememberToken = rememberToken
                };

                // Serialize user data
                var userDataJson = JsonSerializer.Serialize(userData);

                // Create cookie options for remember me
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(REMEMBER_ME_DAYS),
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                };

                // Set the remember me token cookie
                httpContext.Response.Cookies.Append(REMEMBER_COOKIE_NAME, rememberToken, cookieOptions);

                // Set the user data cookie (encrypted)
                var encryptedUserData = EncryptUserData(userDataJson);
                httpContext.Response.Cookies.Append(USER_DATA_COOKIE_NAME, encryptedUserData, cookieOptions);

                _logger.LogInformation("Remember me cookies created successfully for user: {UserId}", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating remember me cookies for user: {UserId}", user.UserId);
            }
        }

        private string GetTokenFromRememberMeCookie()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return null;

                // Check if remember me cookie exists
                var rememberToken = httpContext.Request.Cookies[REMEMBER_COOKIE_NAME];
                if (string.IsNullOrEmpty(rememberToken)) return null;

                // Get and decrypt user data
                var userData = GetUserDataFromRememberMeCookie();
                if (userData == null) return null;

                // Verify the remember token matches
                if (userData.RememberToken != rememberToken) return null;

                // Restore session from cookie data
                RestoreSessionFromCookieData(userData);

                return userData.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token from remember me cookie");
                return null;
            }
        }

        private RememberMeUserData GetUserDataFromRememberMeCookie()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return null;

                var encryptedUserData = httpContext.Request.Cookies[USER_DATA_COOKIE_NAME];
                if (string.IsNullOrEmpty(encryptedUserData)) return null;

                var userDataJson = DecryptUserData(encryptedUserData);
                if (string.IsNullOrEmpty(userDataJson)) return null;

                var userData = JsonSerializer.Deserialize<RememberMeUserData>(userDataJson);

                // Check if the cookie data is still valid (within expiry time)
                if (userData?.LoginTimestamp != null &&
                    DateTime.TryParse(userData.LoginTimestamp, out var loginTime) &&
                    loginTime.AddDays(REMEMBER_ME_DAYS) > DateTime.UtcNow)
                {
                    return userData;
                }

                // Cookie is expired, clear it
                ClearRememberMeCookies();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user data from remember me cookie");
                ClearRememberMeCookies(); // Clear invalid cookies
                return null;
            }
        }

        private void RestoreSessionFromCookieData(RememberMeUserData userData)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null || userData == null) return;

                // Only restore if session is empty
                if (string.IsNullOrEmpty(session.GetString("UserToken")))
                {
                    session.SetString("UserToken", userData.Token);
                    session.SetString("UserName", userData.UserName);
                    session.SetString("UserRole", userData.UserRole);
                    session.SetString("UserId", userData.UserId);
                    session.SetString("ProfileId", userData.ProfileId);

                    if (!string.IsNullOrEmpty(userData.ClientId))
                        session.SetString("ClientId", userData.ClientId);

                    session.SetString("LoginTimestamp", userData.LoginTimestamp);

                    _logger.LogInformation("Session restored from remember me cookies for user: {UserId}", userData.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring session from cookie data for user: {UserId}", userData?.UserId);
            }
        }

        private void ClearRememberMeCookies()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                };

                httpContext.Response.Cookies.Append(REMEMBER_COOKIE_NAME, "", cookieOptions);
                httpContext.Response.Cookies.Append(USER_DATA_COOKIE_NAME, "", cookieOptions);

                _logger.LogInformation("Remember me cookies cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing remember me cookies");
            }
        }

        #endregion

        #region Security Helper Methods

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        private string EncryptUserData(string data)
        {
            try
            {
                // Simple encryption for demo purposes
                // In production, use proper encryption like AES with a key management system
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting user data");
                return null;
            }
        }

        private string DecryptUserData(string encryptedData)
        {
            try
            {
                // Simple decryption for demo purposes
                // In production, use proper decryption like AES with a key management system
                var bytes = Convert.FromBase64String(encryptedData);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting user data");
                return null;
            }
        }

        #endregion
    }

    // Data model for remember me cookie storage
    public class RememberMeUserData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string ProfileId { get; set; }
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string LoginTimestamp { get; set; }
        public string RememberToken { get; set; }
    }
}