using Domain;
using Microsoft.AspNetCore.Http;
using System;

namespace Website.Services
{
    public class AuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(GetToken());

        public string GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserToken");
        }

        public void StoreUserSession(User user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null && user != null)
            {
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
            }
        }

        public void ClearUserSession()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public bool IsUserInRole(string role)
        {
            var userRole = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(userRole) && userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        // Updated user type properties
        public bool IsClient => IsUserInRole("Client");
        public bool IsAdmin => IsUserInRole("Admin");
    }
}