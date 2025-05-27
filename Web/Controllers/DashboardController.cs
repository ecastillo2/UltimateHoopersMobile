using Microsoft.AspNetCore.Mvc;
using Website.Attributes;
using Website.Services;

namespace Web.Controllers
{
    [Authentication] // Require authentication for all actions in this controller
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly AuthenticationService _authenticationService;

        public DashboardController(
            ILogger<DashboardController> logger,
            AuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public IActionResult Dashboard()
        {
            // Pass user type to the view
            ViewData["UserType"] = _authenticationService.IsClient ? "Client" : "Admin";
            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult Client()
        {
            ViewData["UserType"] = "Admin";
            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult User()
        {
            ViewData["UserType"] = "Admin";
            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult Run()
        {
            ViewData["UserType"] = "Admin";
            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult Reports()
        {
            ViewData["UserType"] = "Admin";
            return RedirectToAction("Index", "Reports");
        }
    }
}