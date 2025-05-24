using Microsoft.AspNetCore.Mvc;
using OfficalWebsite.Models;
using System.Diagnostics;
using ApiClient.Authentication;
using Domain;

namespace OfficalWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthenticateUser _authService;

        public HomeController(ILogger<HomeController> logger, IAuthenticateUser authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public IActionResult Index()
        {
            // Check if user is already authenticated and redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                string userRole = HttpContext.Session.GetString("UserRole") ?? "User";
                return RedirectToDashboard(userRole);
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
        public IActionResult AdminDashboard()
        {
            // Check if user is authenticated
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                return RedirectToAction("Login", "Home");
            }

            // Check if user has admin role
            string userRole = HttpContext.Session.GetString("UserRole") ?? "User";
            if (userRole != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Get user info from session
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View();
        }

        [HttpGet]
        public IActionResult ManagerDashboard()
        {
            // Check if user is authenticated
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                return RedirectToAction("Login", "Home");
            }

            // Check if user has manager role
            string userRole = HttpContext.Session.GetString("UserRole") ?? "User";
            if (userRole != "Manager")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Get user info from session
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View();
        }

        [HttpGet]
        public IActionResult UserDashboard()
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

        [HttpGet]
        public IActionResult Login(string returnUrl = null, string error = null)
        {
            // Redirect if already authenticated
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                string userRole = HttpContext.Session.GetString("UserRole") ?? "User";
                return RedirectToDashboard(userRole);
            }

            ViewBag.ReturnUrl = returnUrl;

            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.ErrorMessage = error;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // For demo purposes, allow login with demo accounts without actual API call
                if (IsDemoAccount(model.Email, model.Password))
                {
                    // Create a simulated user for demo purposes
                    var demoUser = CreateDemoUser(model.Email, model.Role);

                    // Store user info in session
                    StoreUserInSession(demoUser, model.Role);

                    _logger.LogInformation("Demo login successful for {Email} as {Role}", model.Email, model.Role);

                    // If there's a return URL, redirect there
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Redirect to appropriate dashboard
                    return RedirectToDashboard(model.Role);
                }

                // Attempt real authentication for non-demo accounts
                var user = await _authService.AuthenticateAsync(model.Email, model.Password);

                if (user != null && !string.IsNullOrEmpty(user.Token))
                {
                    // Store user info in session
                    StoreUserInSession(user, user.AccessLevel ?? model.Role);

                    _logger.LogInformation("Login successful for {Email}", model.Email);

                    // If there's a return URL, redirect there
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Redirect to appropriate dashboard
                    return RedirectToDashboard(user.AccessLevel ?? model.Role);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt. Please check your credentials.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", model.Email);
                ModelState.AddModelError("", $"Login failed: {ex.Message}");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        // Helper methods
        private IActionResult RedirectToDashboard(string role)
        {
            return role switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Home"),
                "Manager" => RedirectToAction("ManagerDashboard", "Home"),
                // This is the important line - make sure we redirect to the Index action, not Dashboard
                _ => RedirectToAction("Index", "Dashboard")
            };
        }

        private void StoreUserInSession(User user, string role)
        {
            HttpContext.Session.SetString("Token", user.Token);
            HttpContext.Session.SetString("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("IsAuthenticated", "true");
        }

        private bool IsDemoAccount(string email, string password)
        {
            // Check if credentials match any of the demo accounts
            return (email == "admin@app.com" && password == "admin123") ||
                   (email == "manager@app.com" && password == "manager123") ||
                   (email == "user@app.com" && password == "user123");
        }

        private User CreateDemoUser(string email, string role)
        {
            // Create a simulated user for demo purposes
            return new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = email,
                FirstName = role, // Use role as first name for demo
                LastName = "Demo",
                Token = $"demo-token-{Guid.NewGuid()}", // Generate a fake token
                AccessLevel = role
            };
        }
    }
}