using ApiClient.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Website.Services;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticateUser _authService;
        private readonly AuthenticationService _authenticationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthenticateUser authService,
            AuthenticationService authenticationService,
            ILogger<AccountController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> ClientLogin(string email, string password, string returnUrl = null)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(email, password);

                if (user == null || string.IsNullOrEmpty(user.Token))
                {
                    TempData["Error"] = "Invalid email or password. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Store user information in session using our service
                _authenticationService.StoreUserSession(user);

                TempData["Success"] = "Successfully logged in as a player!";

                // Redirect to returnUrl if provided, otherwise to dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

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
        public async Task<IActionResult> StaffLogin(string email, string password, string returnUrl = null)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(email, password);

                if (user == null || string.IsNullOrEmpty(user.Token))
                {
                    TempData["Error"] = "Invalid email or password. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Store user information in session using our service
                _authenticationService.StoreUserSession(user);

                TempData["Success"] = "Successfully logged in as a coach!";

                // Redirect to returnUrl if provided, otherwise to dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

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
            // Clear session using our service
            _authenticationService.ClearUserSession();

            TempData["Success"] = "Successfully logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}