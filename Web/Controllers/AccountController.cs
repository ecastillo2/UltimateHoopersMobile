using ApiClient.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;
using Website.Models;
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

                // Force the user role to be Client for client login
                user.AccessLevel = "Client";

                // Store user information in session using our service
                _authenticationService.StoreUserSession(user);

                TempData["Success"] = "Successfully logged in as a client!";

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
        public async Task<IActionResult> AdminLogin(string email, string password, string returnUrl = null)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(email, password);

                if (user == null || string.IsNullOrEmpty(user.Token))
                {
                    TempData["Error"] = "Invalid email or password. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Force the user role to be Admin for admin login
                user.AccessLevel = "Admin";

                // Store user information in session using our service
                _authenticationService.StoreUserSession(user);

                TempData["Success"] = "Successfully logged in as an administrator!";

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

        [HttpPost]
        public async Task<IActionResult> ClientSignup(ClientSignupModel model)
        {
            try
            {
                // Validate the model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["Error"] = $"Please correct the following errors: {string.Join(", ", errors)}";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Check if passwords match
                if (model.Password != model.ConfirmPassword)
                {
                    TempData["Error"] = "Passwords do not match. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Check if email already exists (you'll need to implement this based on your user service)
                // var existingUser = await _authService.GetUserByEmailAsync(model.Email);
                // if (existingUser != null)
                // {
                //     TempData["Error"] = "An account with this email already exists.";
                //     return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                // }

                // Create the client user (implement this based on your user service)
                var newUser = new CreateUserModel
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password,
                    AccessLevel = "Client",
                    AgeGroup = model.AgeGroup,
                    SkillLevel = model.SkillLevel,
                    AcceptTerms = model.AcceptTerms,
                    SubscribeNewsletter = model.SubscribeNewsletter
                };

                // Call your user creation service
                // var createdUser = await _authService.CreateUserAsync(newUser);

                // Send welcome email
                // await _emailService.SendWelcomeEmailAsync(newUser);

                // For demo purposes, simulate successful creation
                _logger.LogInformation("Client signup attempt for email: {Email}", model.Email);

                TempData["Success"] = "Account created successfully! Please check your email to verify your account.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client signup for email: {Email}", model.Email);
                TempData["Error"] = "An error occurred while creating your account. Please try again.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdminSignup(AdminSignupModel model)
        {
            try
            {
                // Validate the model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["Error"] = $"Please correct the following errors: {string.Join(", ", errors)}";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Check if passwords match
                if (model.Password != model.ConfirmPassword)
                {
                    TempData["Error"] = "Passwords do not match. Please try again.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Validate business email (basic check)
                var emailDomain = model.Email.Split('@').LastOrDefault()?.ToLower();
                var personalDomains = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "aol.com", "icloud.com" };

                if (personalDomains.Contains(emailDomain))
                {
                    TempData["Error"] = "Please use your business email address for admin registration.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Check if email already exists
                // var existingUser = await _authService.GetUserByEmailAsync(model.Email);
                // if (existingUser != null)
                // {
                //     TempData["Error"] = "An account with this email already exists.";
                //     return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                // }

                // Create the admin user (implement this based on your user service)
                var newUser = new CreateUserModel
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password,
                    AccessLevel = "Admin",
                    Organization = model.Organization,
                    Role = model.Role,
                    FacilityType = model.FacilityType,
                    AcceptTerms = model.AcceptTerms,
                    SubscribeMarketing = model.SubscribeMarketing,
                    RequiresApproval = true // Admin accounts need approval
                };

                // Call your user creation service
                // var createdUser = await _authService.CreateUserAsync(newUser);

                // Send notification email to admin team about new admin signup
                // await _emailService.SendAdminSignupNotificationAsync(newUser);

                // Send confirmation email to user
                // await _emailService.SendAdminSignupConfirmationAsync(newUser);

                // For demo purposes, simulate successful creation
                _logger.LogInformation("Admin signup attempt for email: {Email}, Organization: {Organization}",
                    model.Email, model.Organization);

                TempData["Success"] = "Admin account request submitted successfully! You'll receive an email within 24 hours with your account status.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin signup for email: {Email}", model.Email);
                TempData["Error"] = "An error occurred while submitting your admin account request. Please try again.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }
        }

        #region Forgot Password Methods

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if user exists (implement this based on your user service)
                // var user = await _authService.GetUserByEmailAsync(model.Email);
                // if (user == null)
                // {
                //     // Don't reveal that user doesn't exist for security
                //     TempData["Success"] = "If an account with that email exists, we've sent a password reset link.";
                //     return View(model);
                // }

                // Generate password reset token
                var resetToken = GeneratePasswordResetToken();
                var resetTokenModel = new PasswordResetTokenModel
                {
                    Email = model.Email,
                    Token = resetToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
                    IsUsed = false
                };

                // Store the reset token (implement this based on your data storage)
                // await _passwordResetService.StoreResetTokenAsync(resetTokenModel);

                // Generate reset link
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token = resetToken, email = model.Email }, Request.Scheme);

                // Send email (implement this based on your email service)
                // await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);

                // For demo purposes, log the reset link
                _logger.LogInformation("Password reset requested for email: {Email}", model.Email);
                _logger.LogInformation("Reset link (FOR DEMO): {ResetLink}", resetLink);

                TempData["Success"] = "If an account with that email exists, we've sent a password reset link.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", model.Email);
                TempData["Error"] = "An error occurred while processing your request. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Invalid password reset link.";
                    return View(new ResetPasswordModel());
                }

                // Validate the reset token (implement this based on your data storage)
                // var resetTokenModel = await _passwordResetService.GetResetTokenAsync(token, email);
                // if (resetTokenModel == null || resetTokenModel.IsUsed || resetTokenModel.ExpiresAt < DateTime.UtcNow)
                // {
                //     TempData["Error"] = "This password reset link has expired or been used. Please request a new one.";
                //     return View(new ResetPasswordModel());
                // }

                // For demo purposes, accept any token that looks valid
                if (token.Length < 32)
                {
                    TempData["Error"] = "Invalid password reset link.";
                    return View(new ResetPasswordModel());
                }

                var model = new ResetPasswordModel
                {
                    Token = token,
                    Email = email
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing reset password page");
                TempData["Error"] = "An error occurred while processing your request.";
                return View(new ResetPasswordModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Validate the reset token again
                // var resetTokenModel = await _passwordResetService.GetResetTokenAsync(model.Token, model.Email);
                // if (resetTokenModel == null || resetTokenModel.IsUsed || resetTokenModel.ExpiresAt < DateTime.UtcNow)
                // {
                //     TempData["Error"] = "This password reset link has expired or been used. Please request a new one.";
                //     return View(model);
                // }

                // Get user and update password (implement this based on your user service)
                // var user = await _authService.GetUserByEmailAsync(model.Email);
                // if (user == null)
                // {
                //     TempData["Error"] = "User not found.";
                //     return View(model);
                // }

                // Update password
                // await _authService.UpdatePasswordAsync(user.Id, model.Password);

                // Mark token as used
                // await _passwordResetService.MarkTokenAsUsedAsync(model.Token);

                // For demo purposes, just log the password reset
                _logger.LogInformation("Password reset completed for email: {Email}", model.Email);

                TempData["Success"] = "Your password has been successfully reset. You can now log in with your new password.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for email: {Email}", model.Email);
                TempData["Error"] = "An error occurred while resetting your password. Please try again.";
                return View(model);
            }
        }

        #endregion

        #region Helper Methods

        private string GeneratePasswordResetToken()
        {
            // Generate a secure random token
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        #endregion

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