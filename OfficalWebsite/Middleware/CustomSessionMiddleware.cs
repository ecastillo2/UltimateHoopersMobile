using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace OfficalWebsite.Middleware
{
    public class CustomSessionMiddleware
    {

        private readonly RequestDelegate _next;

        public CustomSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var token = context.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    // Session has expired, sign out the user
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
