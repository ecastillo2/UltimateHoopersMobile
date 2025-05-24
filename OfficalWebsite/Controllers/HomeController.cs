using Microsoft.AspNetCore.Mvc;
using OfficalWebsite.Models;
using System.Diagnostics;

namespace OfficalWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        public IActionResult Pricing()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Simulate authentication logic
                if (ValidateUser(model.Email, model.Password))
                {
                    // Store user info in session or cookie
                    HttpContext.Session.SetString("UserEmail", model.Email);
                    HttpContext.Session.SetString("UserRole", model.Role);
                    HttpContext.Session.SetString("IsAuthenticated", "true");

                    // Redirect based on role
                    return model.Role switch
                    {
                        "Admin" => RedirectToAction("AdminDashboard"),
                        "Manager" => RedirectToAction("ManagerDashboard"),
                        "User" => RedirectToAction("UserDashboard"),
                        _ => RedirectToAction("Index")
                    };
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                }
            }

            return View(model);
        }

        public IActionResult AdminDashboard()
        {
            if (!IsAuthenticated() || GetUserRole() != "Admin")
                return RedirectToAction("Login");

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        public IActionResult ManagerDashboard()
        {
            if (!IsAuthenticated() || GetUserRole() != "Manager")
                return RedirectToAction("Login");

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        public IActionResult UserDashboard()
        {
            if (!IsAuthenticated() || GetUserRole() != "User")
                return RedirectToAction("Login");

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        private bool ValidateUser(string email, string password)
        {
            // Simple demo validation - replace with real authentication
            var validUsers = new Dictionary<string, string>
            {
                { "admin@app.com", "admin123" },
                { "manager@app.com", "manager123" },
                { "user@app.com", "user123" }
            };

            return validUsers.ContainsKey(email) && validUsers[email] == password;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}