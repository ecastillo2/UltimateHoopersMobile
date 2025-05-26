// In Attributes/AuthenticationAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Website.Services;

namespace Website.Attributes
{
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthenticationAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? Array.Empty<string>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<AuthenticationService>();

            if (authService == null || !authService.IsAuthenticated)
            {
                // Store the current URL to redirect back after login
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;

                // Redirect to login
                context.Result = new RedirectToActionResult("Index", "Home", new { scrollTo = "login", returnUrl });
                return;
            }

            // Check roles if any specified
            if (_allowedRoles.Length > 0)
            {
                bool isInRole = false;
                foreach (var role in _allowedRoles)
                {
                    if (authService.IsUserInRole(role))
                    {
                        isInRole = true;
                        break;
                    }
                }

                if (!isInRole)
                {
                    // Not authorized for this action
                    context.Result = new RedirectToActionResult("Dashboard", "Dashboard", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}