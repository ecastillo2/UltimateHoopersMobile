using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            // In a real app, we would check if the user is authenticated here
            // For demo, we'll just check if there's a Success message indicating login
            if (TempData["Success"]?.ToString()?.Contains("logged in") != true)
            {
                // Not logged in, redirect to home
                TempData["Error"] = "You must be logged in to access the dashboard.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            // For this demo, we'll create a view model based on login type
            bool isPlayer = TempData["Success"]?.ToString()?.Contains("player") == true;

            // Pass the login type to the view
            ViewData["UserType"] = isPlayer ? "Player" : "Coach";

            // In a real app, we would load user-specific data here
            return View();
        }

        public IActionResult Client()
        {
            //// Check login status similar to Dashboard action
            //if (TempData["Success"]?.ToString()?.Contains("logged in") != true)
            //{
            //    // Not logged in, redirect to home
            //    TempData["Error"] = "You must be logged in to access the client management.";
            //    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            //}

            //// Check if user is a coach (only coaches should access client management)
            bool isPlayer = TempData["Success"]?.ToString()?.Contains("player") == true;
            if (isPlayer)
            {
                TempData["Error"] = "Players cannot access the client management section.";
                return RedirectToAction("Dashboard");
            }

            ViewData["UserType"] = "Coach";

            // In a real app, we would fetch client data here
            // For demo, we'll use hard-coded data in the view
            return View();
        }

        public IActionResult User()
        {
            // Check login status similar to other actions
            if (TempData["Success"]?.ToString()?.Contains("logged in") != true)
            {
                // Not logged in, redirect to home
                TempData["Error"] = "You must be logged in to access the user management.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            // Check if user is a coach (only coaches should access user management)
            bool isPlayer = TempData["Success"]?.ToString()?.Contains("player") == true;
            if (isPlayer)
            {
                TempData["Error"] = "Players cannot access the user management section.";
                return RedirectToAction("Dashboard");
            }

            ViewData["UserType"] = "Coach";

            // In a real app, we would fetch user data here
            // For demo, we'll use hard-coded data in the view
            return View();
        }
    }
}