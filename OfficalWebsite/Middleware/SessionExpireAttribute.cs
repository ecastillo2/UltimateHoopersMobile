using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace OfficalWebsite.Middleware
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token = filterContext.HttpContext.Session.GetString("Token");

            // Check if the session has expired
            if (string.IsNullOrEmpty(token))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                    { "controller", "Home" },
                    { "action", "Index" }
                    });
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
