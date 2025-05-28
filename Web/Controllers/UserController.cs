using ApiClient;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.ApiClients;
using Website.ViewModels;

namespace Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserApi _userApi;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserApi userApi,
            ILogger<UserController> logger)
        {
            _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> User(
            string cursor = null,
            int limit = 10,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view users.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get current user role and profile ID
                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                // Get users with cursor pagination
                var result = await _userApi.GetUsersWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                // Create view model
                var viewModel = new UsersViewModel
                {
                    Users = result.Items,
                    NextCursor = result.NextCursor,
                    CurrentSortBy = sortBy,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                TempData["Error"] = "An error occurred while retrieving users. Please try again later.";
                return RedirectToAction("Dashboard", "Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view user details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get User details
                var user = await _userApi.GetUserByIdAsync(id, accessToken, cancellationToken);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("User");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for ID: {UserId}", id);
                TempData["Error"] = "An error occurred while retrieving user details. Please try again later.";
                return RedirectToAction("User");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a user.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a user.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Set the current date if not provided
                if (user.SignUpDate == default)
                {
                    user.SignUpDate = DateTime.UtcNow;
                }

                // Create new user
                var createdUser = await _userApi.CreateUserAsync(user, accessToken, cancellationToken);

                TempData["Success"] = "User created successfully.";
                return RedirectToAction("User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                TempData["Error"] = "An error occurred while creating the user. Please try again later.";
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a user.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get user details
                var user = await _userApi.GetUserByIdAsync(id, accessToken, cancellationToken);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("User");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for edit, ID: {UserId}", id);
                TempData["Error"] = "An error occurred while retrieving the user. Please try again later.";
                return RedirectToAction("User");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a user.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Update user
                await _userApi.UpdateUserAsync(user, accessToken, cancellationToken);

                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
                TempData["Error"] = "An error occurred while updating the user. Please try again later.";
                return View(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to delete a user.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get user details first to check permissions
                var user = await _userApi.GetUserByIdAsync(id, accessToken, cancellationToken);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("User");
                }

                // Delete user
                await _userApi.DeleteUserAsync(id, accessToken, cancellationToken);

                TempData["Success"] = "User deleted successfully.";
                return RedirectToAction("User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                TempData["Error"] = "An error occurred while deleting the user. Please try again later.";
                return RedirectToAction("User");
            }
        }

        // User data retrieval for AJAX requests
        [HttpGet]
        public async Task<IActionResult> GetUserData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Get user details
                var user = await _userApi.GetUserByIdAsync(id, accessToken, cancellationToken);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Format the response with all needed user properties
                var userData = new
                {
                    success = true,
                    user = new
                    {
                        userId = user.UserId,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email,
                        phoneNumber = user.PhoneNumber,

                        accessLevel = user.AccessLevel,
                        status = user.Status ?? "Active",
                        signUpDate = user.SignUpDate,
                        profileId = user.ProfileId,
                    
                    }
                };

                return Json(userData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user data for ID: {UserId}", id);
                return Json(new { success = false, message = "Error retrieving user data" });
            }
        }

        // User data retrieval for profile tab
        [HttpGet]
        public IActionResult GetUserProfileData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // In a real implementation, you would fetch this from your database
                // For now, return mock data based on the ID to make it look different
                var mockNumber = string.IsNullOrEmpty(id) ? 0 : int.Parse(id.Substring(0, 1));

                var profile = new
                {
                    firstName = "John",
                    lastName = "Doe" + (mockNumber > 0 ? " " + mockNumber : ""),
                    email = "john.doe" + mockNumber + "@example.com",
                    phoneNumber = "(555) " + (123 + mockNumber) + "-4567",
                    address = (123 + mockNumber) + " Main St",
                    city = "Basketball City",
                    state = "CA",
                    zip = "9021" + mockNumber,
                    accessLevel = mockNumber % 2 == 0 ? "Player" : "Coach",
                    status = "Active",
                    signUpDate = DateTime.UtcNow.AddMonths(-3 - mockNumber),
                    lastLoginDate = DateTime.UtcNow.AddDays(-2 - mockNumber),
                    usageStats = new { usagePercent = 75 - (mockNumber * 5) },
                    stats = new { runsJoined = 12 - mockNumber, runsHosted = 3 + mockNumber, achievements = 5 }
                };

                return Json(new { success = true, profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile data for ID: {UserId}", id);
                return Json(new { success = false, message = "Error retrieving profile data: " + ex.Message });
            }
        }

        // User activity retrieval for timeline
        [HttpGet]
        public IActionResult GetUserActivity(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // In a real implementation, you would fetch this from your database
                // For now, return mock data
                var activities = new List<object>
                {
                    new {
                        type = "login",
                        title = "Logged In",
                        description = "User logged into the platform",
                        timestamp = DateTime.UtcNow.AddDays(-1)
                    },
                    new {
                        type = "join_run",
                        title = "Joined Run",
                        description = "Joined 'Saturday Morning Pickup' run",
                        timestamp = DateTime.UtcNow.AddDays(-3)
                    },
                    new {
                        type = "create_run",
                        title = "Created Run",
                        description = "Created 'Weeknight Practice' run",
                        timestamp = DateTime.UtcNow.AddDays(-5)
                    },
                    new {
                        type = "profile_update",
                        title = "Updated Profile",
                        description = "Updated personal information",
                        timestamp = DateTime.UtcNow.AddDays(-7)
                    },
                    new {
                        type = "achievement",
                        title = "Earned Achievement",
                        description = "Earned 'Consistent Player' badge",
                        timestamp = DateTime.UtcNow.AddDays(-10)
                    }
                };

                return Json(new { activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activity for ID: {UserId}", id);
                return Json(new { success = false, message = "Error retrieving user activity: " + ex.Message });
            }
        }

        // Scouting report retrieval
        [HttpGet]
        public IActionResult GetScoutingReport(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // In a real implementation, you would fetch this from your database
                // For now, return mock data based on the ID to simulate different data
                var hasExistingReport = !string.IsNullOrEmpty(id) && id.Length > 0 && char.IsDigit(id[0]) && int.Parse(id[0].ToString()) % 2 == 0;

                var scoutingReport = new
                {
                    scoutingReportId = hasExistingReport ? "SR-" + id : "",
                    height = "6'2\"",
                    weight = 185,
                    wingspan = "6'5\"",
                    verticalJump = 32,
                    primaryPosition = "SG",
                    secondaryPosition = "PG",
                    playingStyle = "Shooter",
                    experienceLevel = "Intermediate",
                    shooting = 8,
                    ballHandling = 7,
                    passing = 6,
                    defense = 7,
                    rebounding = 5,
                    athleticism = 8,
                    strengths = "Great shooter with quick release. Good ball handler with ability to create own shot.",
                    weaknesses = "Needs to improve defensive positioning and rebounding technique.",
                    scoutingNotes = "Shows potential as a combo guard. Work on decision making in transition.",
                    evaluatedBy = hasExistingReport ? "Coach Mike" : null,
                    evaluationDate = hasExistingReport ? DateTime.UtcNow.AddDays(-14) : (DateTime?)null,
                    lastUpdated = hasExistingReport ? DateTime.UtcNow.AddDays(-7) : (DateTime?)null
                };

                return Json(new { success = true, scoutingReport });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scouting report for ID: {UserId}", id);
                return Json(new { success = false, message = "Error retrieving scouting report: " + ex.Message });
            }
        }

        // Update scouting report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateScoutingReport([FromBody] object scoutingReportData, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // In a real implementation, you would save this to your database
                // For now, just return success with the data we would have saved
                var scoutingReport = new
                {
                    scoutingReportId = "SR-" + Guid.NewGuid().ToString().Substring(0, 8),
                    evaluatedBy = "Coach " + (HttpContext.Session.GetString("UserName")?.Split(' ')[0] ?? "Admin"),
                    evaluationDate = DateTime.UtcNow,
                    lastUpdated = DateTime.UtcNow
                };

                return Json(new { success = true, message = "Scouting report saved successfully", scoutingReport });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving scouting report");
                return Json(new { success = false, message = "Error saving scouting report: " + ex.Message });
            }
        }
    }
}