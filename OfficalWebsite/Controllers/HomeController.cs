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
            // Check if user is already authenticated and redirect to appropriate dashboard
            if (IsAuthenticated())
            {
                var userRole = GetUserRole();
                return userRole switch
                {
                    "Admin" => RedirectToAction("AdminDashboard"),
                    "Manager" => RedirectToAction("ManagerDashboard"),
                    "User" => RedirectToAction("UserDashboard"),
                    _ => View()
                };
            }

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

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Redirect if already authenticated
            if (IsAuthenticated())
            {
                var userRole = GetUserRole();
                return userRole switch
                {
                    "Admin" => RedirectToAction("AdminDashboard"),
                    "Manager" => RedirectToAction("ManagerDashboard"),
                    "User" => RedirectToAction("UserDashboard"),
                    _ => RedirectToAction("Index")
                };
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Simulate authentication logic
                if (ValidateUser(model.Email, model.Password, model.Role))
                {
                    // Store user info in session
                    HttpContext.Session.SetString("UserEmail", model.Email);
                    HttpContext.Session.SetString("UserRole", model.Role);
                    HttpContext.Session.SetString("IsAuthenticated", "true");
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString());
                    HttpContext.Session.SetString("UserId", GetUserIdByEmail(model.Email));

                    // Log the login
                    _logger.LogInformation("User {Email} logged in as {Role} at {Time}",
                        model.Email, model.Role, DateTime.Now);

                    // Set success message
                    TempData["SuccessMessage"] = $"Welcome back! You have successfully logged in as {model.Role}.";

                    // Redirect to return URL or appropriate dashboard
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

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
                    ModelState.AddModelError(string.Empty, "Invalid email, password, or role combination. Please check your credentials and try again.");
                    _logger.LogWarning("Failed login attempt for {Email} as {Role} at {Time}",
                        model.Email, model.Role, DateTime.Now);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            if (!IsAuthenticated() || GetUserRole() != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Login", new { returnUrl = "/Home/AdminDashboard" });
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.LoginTime = HttpContext.Session.GetString("LoginTime");
            ViewBag.PageTitle = "System Administration";

            return View();
        }

        [HttpGet]
        public IActionResult ManagerDashboard()
        {
            if (!IsAuthenticated() || GetUserRole() != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied. Manager privileges required.";
                return RedirectToAction("Login", new { returnUrl = "/Home/ManagerDashboard" });
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.LoginTime = HttpContext.Session.GetString("LoginTime");
            ViewBag.PageTitle = "Team Management";

            return View();
        }

        [HttpGet]
        public IActionResult UserDashboard()
        {
            if (!IsAuthenticated() || GetUserRole() != "User")
            {
                TempData["ErrorMessage"] = "Access denied. Please log in to access your dashboard.";
                return RedirectToAction("Login", new { returnUrl = "/Home/UserDashboard" });
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.LoginTime = HttpContext.Session.GetString("LoginTime");
            ViewBag.PageTitle = "My Workspace";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Clear session
            HttpContext.Session.Clear();

            // Log the logout
            _logger.LogInformation("User {Email} ({Role}) logged out at {Time}",
                userEmail, userRole, DateTime.Now);

            // Set logout message
            TempData["InfoMessage"] = "You have been successfully logged out. Thank you for using TaskFlow Pro!";

            return RedirectToAction("Index");
        }

        // Helper method to validate user credentials
        private bool ValidateUser(string email, string password, string role)
        {
            // Simple demo validation - replace with real authentication
            var validUsers = new Dictionary<string, (string password, string role)>
            {
                { "admin@app.com", ("admin123", "Admin") },
                { "manager@app.com", ("manager123", "Manager") },
                { "user@app.com", ("user123", "User") }
            };

            if (validUsers.TryGetValue(email, out var userInfo))
            {
                return userInfo.password == password && userInfo.role == role;
            }

            return false;
        }

        // Helper method to get user ID by email
        private string GetUserIdByEmail(string email)
        {
            var userIds = new Dictionary<string, string>
            {
                { "admin@app.com", "USR001" },
                { "manager@app.com", "USR002" },
                { "user@app.com", "USR003" }
            };

            return userIds.TryGetValue(email, out var userId) ? userId : Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        // Check if user is authenticated
        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        // Get current user role
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