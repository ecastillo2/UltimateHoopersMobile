using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebAdmin.Models;
using WebAdmin.Services;

namespace WebAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;

        public AccountController(ILogger<AccountController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> ClientLogin(string email, string password)
        {
            // Validate client credentials
            if (_userService.ValidateClientCredentials(email, password))
            {
                // Get user details
                var user = _userService.GetUserByEmail(email);
                string displayName = user?.Name ?? email;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Client")
                };

                if (user != null)
                {
                    claims.Add(new Claim("ProfileImage", user.ProfileImage));
                    claims.Add(new Claim("UserId", user.Id));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Email} logged in as Client", email);
                return RedirectToAction("Dashboard", "Home");
            }

            TempData["Error"] = "Invalid email or password";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> StaffLogin(string email, string password)
        {
            // Validate staff credentials
            if (_userService.ValidateStaffCredentials(email, password))
            {
                // Get user details
                var user = _userService.GetUserByEmail(email);
                string displayName = user?.Name ?? email;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Staff")
                };

                if (user != null)
                {
                    claims.Add(new Claim("ProfileImage", user.ProfileImage));
                    claims.Add(new Claim("UserId", user.Id));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Email} logged in as Staff", email);
                return RedirectToAction("Dashboard", "Home");
            }

            TempData["Error"] = "Invalid email or password";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}