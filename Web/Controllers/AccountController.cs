using ApiClient.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticateUser _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthenticateUser authService,
            ILogger<AccountController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> ClientLogin(string email, string password)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(email, password);

                if (user == null || string.IsNullOrEmpty(user.Token))
                {
                    TempData["Error"] = "Invalid email or password. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Store user information in session
                HttpContext.Session.SetString("UserToken", user.Token);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                HttpContext.Session.SetString("UserRole", user.AccessLevel);
                HttpContext.Session.SetString("UserId", user.UserId);
                HttpContext.Session.SetString("ProfileId", user.ProfileId);
                HttpContext.Session.SetString("ClientId", user.ClientId);

                TempData["Success"] = "Successfully logged in as a player!";
                return RedirectToAction("Dashboard", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client login for email: {Email}", email);
                TempData["Error"] = "Invalid email or password. Please try again.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StaffLogin(string email, string password)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(email, password);

                if (user == null || string.IsNullOrEmpty(user.Token))
                {
                    TempData["Error"] = "Invalid email or password. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Store user information in session
                HttpContext.Session.SetString("UserToken", user.Token);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                HttpContext.Session.SetString("UserRole", user.AccessLevel);
                HttpContext.Session.SetString("UserId", user.UserId);
                HttpContext.Session.SetString("ProfileId", user.ProfileId);
                HttpContext.Session.SetString("Subscription", user.Subscription);
                

                TempData["Success"] = "Successfully logged in as a coach!";
                return RedirectToAction("Dashboard", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during staff login for email: {Email}", email);
                TempData["Error"] = "Invalid email or password. Please try again.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            TempData["Success"] = "Successfully logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}