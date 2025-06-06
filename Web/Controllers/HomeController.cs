using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Models;
using Website.Services;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthenticationService _authenticationService;

        public HomeController(ILogger<HomeController> logger,AuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public IActionResult Index()
        {
            // If user is logged in, redirect to dashboard
            if (_authenticationService.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }

            return View();
        }

        public IActionResult Blog()
        {
            return View();
        }

        public IActionResult SingleBlog()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}