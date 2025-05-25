using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult ClientLogin(string email, string password)
        {
            // Simple demo validation - in a real app, this would check a database
            if (email?.EndsWith("@client.com", StringComparison.OrdinalIgnoreCase) == true && password == "client123")
            {
                // Success - in a real app, this would set authentication cookies
                TempData["Success"] = "Successfully logged in as a player!";
                return RedirectToAction("Dashboard", "Dashboard");
            }

            // Failed login
            TempData["Error"] = "Invalid email or password. Please try again.";
            return RedirectToAction("Index", "Home", new { scrollTo = "login" });
        }

        [HttpPost]
        public IActionResult StaffLogin(string email, string password)
        {
            // Simple demo validation - in a real app, this would check a database
            if (email?.EndsWith("@company.com", StringComparison.OrdinalIgnoreCase) == true && password == "staff123")
            {
                // Success - in a real app, this would set authentication cookies
                TempData["Success"] = "Successfully logged in as a coach!";
                return RedirectToAction("Dashboard", "Dashboard");
            }

            // Failed login
            TempData["Error"] = "Invalid email or password. Please try again.";
            return RedirectToAction("Index", "Home", new { scrollTo = "login" });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // In a real app, this would clear authentication cookies
            TempData["Success"] = "Successfully logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}