using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.ApiClients;
using Website.ViewModels;

namespace Web.Controllers
{
    public class RunController : Controller
    {
        private readonly IRunApi _runApi;
        private readonly ILogger<RunController> _logger;

        public RunController(
            IRunApi runApi,
            ILogger<RunController> logger)
        {
            _runApi = runApi ?? throw new ArgumentNullException(nameof(runApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Run(
            string cursor = null,
            int limit = 10,
            string direction = "next",
            string sortBy = "StartDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view runs.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get current user role and profile ID
                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                // Get runs with cursor pagination
                var result = await _runApi.GetRunsWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                // Create view model
                var viewModel = new RunsViewModel
                {
                    Runs = result.Items,
                    NextCursor = result.NextCursor,
                    //PreviousCursor = result.PreviousCursor,
                    //HasMore = result.HasMore,
                    //TotalCount = result.TotalCount,
                    //CurrentLimit = limit,
                    CurrentSortBy = sortBy,
                    //UserRole = userRole,
                    //ProfileId = profileId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runs");
                TempData["Error"] = "An error occurred while retrieving runs. Please try again later.";
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
                    TempData["Error"] = "You must be logged in to view run details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get run details
                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                // Format the response with all needed user properties
                var runData = new
                {
                    success = true,
                    run = new
                    {
                        CourtId = run.CourtId,
                        ProfileId = run.ProfileId,
                        ClientId = run.ClientId,
                        Status = run.Status,
                        RunDate = run.RunDate,
                        Cost = run.Cost,
                        Description = run.Description ?? "Active",
                        CreatedDate = run.CreatedDate,
                        Name = run.Name,
                        StartTime = run.StartTime,
                        EndTime = run.EndTime,
                        Type = run.Type,
                        RunNumber = run.RunNumber,
                        SkillLevel = run.SkillLevel,
                        PaymentMethod = run.PaymentMethod,
                        PlayerLimit = run.PlayerLimit,
                        TeamType = run.TeamType,
                        IsPublic = run.IsPublic,
                    }
                };

                return Json(runData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run details for ID: {RunId}", id);
                TempData["Error"] = "An error occurred while retrieving run details. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a run.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Run());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Run run, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Set creator profile ID
                run.ProfileId = HttpContext.Session.GetString("ProfileId");

                // Create new run
                var createdRun = await _runApi.CreateRunAsync(run, accessToken, cancellationToken);

                TempData["Success"] = "Run created successfully.";
                return RedirectToAction("Details", new { id = createdRun.RunId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating run");
                TempData["Error"] = "An error occurred while creating the run. Please try again later.";
                return View(run);
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
                    TempData["Error"] = "You must be logged in to edit a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get run details
                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to edit this run.";
                    return RedirectToAction("Details", new { id = id });
                }

                return View(run);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run for edit, ID: {RunId}", id);
                TempData["Error"] = "An error occurred while retrieving the run. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Run run, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to edit this run.";
                    return RedirectToAction("Details", new { id = run.RunId });
                }

                // Update run
                await _runApi.UpdateRunAsync(run, accessToken, cancellationToken);

                TempData["Success"] = "Run updated successfully.";
                return RedirectToAction("Details", new { id = run.RunId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating run: {RunId}", run.RunId);
                TempData["Error"] = "An error occurred while updating the run. Please try again later.";
                return View(run);
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
                    TempData["Error"] = "You must be logged in to delete a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get run details first to check permissions
                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to delete this run.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Delete run
                await _runApi.DeleteRunAsync(id, accessToken, cancellationToken);

                TempData["Success"] = "Run deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting run: {RunId}", id);
                TempData["Error"] = "An error occurred while deleting the run. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string runId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to join a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get profile ID
                var profileId = HttpContext.Session.GetString("ProfileId");
                if (string.IsNullOrEmpty(profileId))
                {
                    TempData["Error"] = "User profile not found.";
                    return RedirectToAction("Details", new { id = runId });
                }

                // Create joined run request
                var joinedRun = new CreateJoinedRunDto
                {
                    RunId = runId,
                    ProfileId = profileId
                };

                // Join run
                await _runApi.UserJoinRunAsync(joinedRun, accessToken, cancellationToken);

                TempData["Success"] = "You have successfully joined the run.";
                return RedirectToAction("Details", new { id = runId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining run: {RunId}", runId);
                TempData["Error"] = "An error occurred while joining the run. Please try again later.";
                return RedirectToAction("Details", new { id = runId });
            }
        }
    }

    // View model for the Runs index page
    
}