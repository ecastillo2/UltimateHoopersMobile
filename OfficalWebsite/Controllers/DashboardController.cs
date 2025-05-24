using Microsoft.AspNetCore.Mvc;
using ApiClient.Authentication;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Domain;

namespace OfficalWebsite.Controllers
{
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
            ViewBag.Email = HttpContext.Session.GetString("Email");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                // For testing, allow empty credentials - simulate login
                var user = new User
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = email ?? "test@example.com",
                    Token = "test-token-123456789",
                    UserId = "TEST001",
                    AccessLevel = "User"
                };

                // Store user info in session
                StoreUserInSession(user);

                return RedirectToAction("Index");
            }

            try
            {
                // Attempt real authentication
                var user = await _authService.AuthenticateAsync(email, password);

                if (user != null && !string.IsNullOrEmpty(user.Token))
                {
                    // Store user info in session
                    StoreUserInSession(user);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ViewBag.ErrorMessage = "Authentication failed: " + ex.Message;
            }

            // For demo, return to login with error
            return RedirectToAction("Login", "Home", new { error = "Invalid credentials" });
        }

        private void StoreUserInSession(User user)
        {
            HttpContext.Session.SetString("Token", user.Token);
            HttpContext.Session.SetString("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.AccessLevel);
        }

        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}