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
            ViewData["UserType"] = _authenticationService.IsPlayer ? "Player" : "Coach";
            return View();
        }

        [Authentication("Coach", "Administrator")] // Only allow coaches and administrators
        public IActionResult Client()
        {
            ViewData["UserType"] = "Coach";
            return View();
        }

        [Authentication("Coach", "Administrator")] // Only allow coaches and administrators
        public IActionResult User()
        {
            ViewData["UserType"] = "Coach";
            return View();
        }

        [Authentication("Coach", "Administrator")] // Only allow coaches and administrators
        public IActionResult Run()
        {
            ViewData["UserType"] = "Coach";
            return View();
        }
    }
}