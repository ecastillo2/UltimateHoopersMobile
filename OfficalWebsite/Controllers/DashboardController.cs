using Microsoft.AspNetCore.Mvc;
using ApiClient.Authentication;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Domain;
using OfficalWebsite.Middleware;

namespace OfficalWebsite.Controllers
{
    [SessionExpire] // Apply the session expiration check
    public class DashboardController : Controller
    {
        private readonly IAuthenticateUser _authService;
        private readonly IConfiguration _configuration;

        public DashboardController(IAuthenticateUser authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Check if user is authenticated
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                return RedirectToAction("Login", "Home");
            }

            // Get user info from session
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            // Determine which dashboard to display based on user role
            string userRole = HttpContext.Session.GetString("UserRole") ?? "User";

            return userRole switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Home"),
                "Manager" => RedirectToAction("ManagerDashboard", "Home"),
                _ => View() // Default User dashboard
            };
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Check if user is authenticated
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                return RedirectToAction("Login", "Home");
            }

            // Get user info from session
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View();
        }

        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}