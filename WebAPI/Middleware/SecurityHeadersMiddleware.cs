// File: WebAPI/Middleware/SecurityHeadersMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebAPI.Middleware
{
    /// <summary>
    /// Middleware to add security headers to HTTP responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Content Security Policy
            context.Response.Headers.Add("Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "connect-src 'self' https://api.undergroundhoopers.com;");

            // X-Content-Type-Options
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // X-Frame-Options
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // X-XSS-Protection
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Referrer-Policy
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Strict-Transport-Security (HSTS)
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

            // Feature-Policy
            context.Response.Headers.Add("Feature-Policy",
                "camera 'none'; microphone 'none'; geolocation 'self'");

            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}