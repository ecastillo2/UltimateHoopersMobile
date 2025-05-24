using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace OfficalWebsite.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Exclude the Home/Index page and static files from middleware processing
            if (context.Request.Path == "/Home/Index" || context.Request.Path.StartsWithSegments("/static"))
            {
                await _next(context);
                return;
            }

            var isVisited = context.Session.GetString("hasVisited");
            if (isVisited != "true")
            {
                // First-time visit logic
                context.Session.SetString("hasVisited", "true");
                var redirectLink = context.Request.Path + context.Request.QueryString; // Preserve query params
                var welcomeScreenUrl = $"/Home/WelcomeScreen?redirectLink={Uri.EscapeDataString(redirectLink)}";

                context.Response.Redirect(welcomeScreenUrl);
                return;
            }

            if (context.Request.Path == "/Account/LogOut/")
            {
                // Logout handling
                context.Session.Clear();
                ClearCookies(context);
                context.Response.Redirect("/Home/Index");
                return;
            }

            if (!context.Session.Keys.Any())
            {
                // Redirect to login if no session data exists
                RedirectToLogin(context);
                return;
            }

            UserSession userSession = LoadUserSession(context);

            if (string.IsNullOrEmpty(userSession.AuthToken))
            {
                // Redirect to login if token is missing
                RedirectToLogin(context);
                return;
            }
            else
            {
                // Check if token is expired
                if (IsTokenExpired(userSession.AuthToken))
                {
                    RedirectToLogin(context);
                    return;
                }
            }

            // Allow user to proceed
            await _next(context);
        }

        private bool IsTokenExpired(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "exp")?.Value;
                if (long.TryParse(expClaim, out var exp))
                {
                    var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    return expirationDate < DateTime.UtcNow;
                }
            }

            return true;
        }

        private void RedirectToLogin(HttpContext context)
        {
            if (context.Request.Path != "/Home/Index")
            {
                var redirectLink = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                var loginUrl = $"/Home/Index?redirectLink={redirectLink}";
                context.Response.Redirect(loginUrl);
            }
        }

        private void ClearCookies(HttpContext context)
        {
            var cookiesToClear = new[]
            {
                "AuthToken", "ProfileId", "TimeZone", "UserId", "FirstName",
                "LastName", "Email", "AccessLevel", "UserName", "ProfileImage",
                "PlayerNumber", "Points", "PostCount", "SavedPostCount",
                "FollowersCount", "FollowingCount", "StarRating"
            };

            foreach (var cookie in cookiesToClear)
            {
                context.Response.Cookies.Delete(cookie);
            }
        }

        private UserSession LoadUserSession(HttpContext context)
        {
            return new UserSession
            {
                AuthToken = context.Session.GetString("Token") ?? context.Request.Cookies["AuthToken"],
                ProfileId = context.Session.GetString("ProfileId") ?? context.Request.Cookies["ProfileId"],
                TimeZone = context.Session.GetString("TimeZone") ?? context.Request.Cookies["TimeZone"],
                UserId = context.Session.GetString("UserId") ?? context.Request.Cookies["UserId"],
                FirstName = context.Session.GetString("FirstName") ?? context.Request.Cookies["FirstName"],
                LastName = context.Session.GetString("LastName") ?? context.Request.Cookies["LastName"],
                Email = context.Session.GetString("Email") ?? context.Request.Cookies["Email"],
                AccessLevel = context.Session.GetString("AccessLevel") ?? context.Request.Cookies["AccessLevel"],
                UserName = context.Session.GetString("UserName") ?? context.Request.Cookies["UserName"],
                ProfileImage = context.Session.GetString("ProfileImage") ?? context.Request.Cookies["ProfileImage"],
                PlayerNumber = context.Session.GetString("PlayerNumber") ?? context.Request.Cookies["PlayerNumber"],
                Points = context.Session.GetString("Points") ?? context.Request.Cookies["Points"],
                PostCount = context.Session.GetString("PostCount") ?? context.Request.Cookies["PostCount"],
                SavedPostCount = context.Session.GetString("SavedPostCount") ?? context.Request.Cookies["SavedPostCount"],
                FollowersCount = context.Session.GetString("FollowersCount") ?? context.Request.Cookies["FollowersCount"],
                FollowingCount = context.Session.GetString("FollowingCount") ?? context.Request.Cookies["FollowingCount"],
                StarRating = context.Session.GetString("StarRating") ?? context.Request.Cookies["StarRating"]
            };
        }
    }

    public class UserSession
    {
        public string AuthToken { get; set; } = string.Empty;
        public string ProfileId { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public string PlayerNumber { get; set; } = string.Empty;
        public string Points { get; set; } = string.Empty;
        public string PostCount { get; set; } = string.Empty;
        public string SavedPostCount { get; set; } = string.Empty;
        public string FollowersCount { get; set; } = string.Empty;
        public string FollowingCount { get; set; } = string.Empty;
        public string StarRating { get; set; } = string.Empty;
    }
}
