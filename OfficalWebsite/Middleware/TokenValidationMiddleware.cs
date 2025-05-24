using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            // Skip authentication check for public pages and static files
            var path = context.Request.Path.ToString().ToLower();
            if (IsPublicPath(path) || path.StartsWith("/static") || path.StartsWith("/lib") || path.StartsWith("/css") || path.StartsWith("/js"))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            var token = context.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                // Not authenticated, redirect to login
                RedirectToLogin(context);
                return;
            }

            // For demo tokens, skip validation and proceed
            if (token.StartsWith("demo-token-"))
            {
                await _next(context);
                return;
            }

            // For real tokens, validate expiration
            if (IsTokenExpired(token))
            {
                // Token expired, clear session and redirect to login
                context.Session.Clear();
                RedirectToLogin(context);
                return;
            }

            // Token is valid, proceed
            await _next(context);
        }

        private bool IsPublicPath(string path)
        {
            var publicPaths = new[]
            {
                "/",
                "/home",
                "/home/index",
                "/home/features",
                "/home/pricing",
                "/home/privacy",
                "/home/login",
                "/home/logout",
                "/account/login",
                "/account/register",
                "/account/forgotpassword"
            };

            return publicPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                // Check if token is in JWT format
                if (!handler.CanReadToken(token))
                {
                    return true; // Consider invalid tokens as expired
                }

                var jwtToken = handler.ReadJwtToken(token);

                // Get expiration time
                var expClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "exp")?.Value;
                if (long.TryParse(expClaim, out var exp))
                {
                    var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    return expirationDate < DateTime.UtcNow;
                }

                return true; // No expiration claim found, consider it expired
            }
            catch
            {
                return true; // Any error in token validation should be treated as expired
            }
        }

        private void RedirectToLogin(HttpContext context)
        {
            // Capture the current URL for redirect after login
            var currentUrl = context.Request.Path + context.Request.QueryString;

            // Don't capture login or logout URLs
            if (!currentUrl.ToString().Contains("/login", StringComparison.OrdinalIgnoreCase) &&
                !currentUrl.ToString().Contains("/logout", StringComparison.OrdinalIgnoreCase))
            {
                var loginUrl = $"/Home/Login?returnUrl={Uri.EscapeDataString(currentUrl)}";
                context.Response.Redirect(loginUrl);
            }
            else
            {
                context.Response.Redirect("/Home/Login");
            }
        }
    }
}