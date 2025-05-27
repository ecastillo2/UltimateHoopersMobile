using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace UnitTest.Utils
{
    public static class TestUtilities
    {
        /// <summary>
        /// Creates a controller context with the specified user claims
        /// </summary>
        public static ControllerContext CreateControllerContext(string userId = "test-user-id", string role = "Standard")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}