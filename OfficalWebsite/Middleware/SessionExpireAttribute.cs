using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace OfficalWebsite.Middleware
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token = filterContext.HttpContext.Session.GetString("Token");

            // Check if the session has expired or is missing authentication token
            if (string.IsNullOrEmpty(token))
            {
                // Store current URL as return URL if it's not a login or logout action
                var currentAction = filterContext.RouteData.Values["action"]?.ToString()?.ToLower();
                var currentController = filterContext.RouteData.Values["controller"]?.ToString()?.ToLower();

                if (currentAction != "login" && currentAction != "logout")
                {
                    var currentUrl = filterContext.HttpContext.Request.Path;
                    // Redirect to login with return URL
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Login" },
                            { "returnUrl", currentUrl }
                        });
                }
                else
                {
                    // Just redirect to login
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Login" }
                        });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}