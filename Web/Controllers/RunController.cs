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
        private readonly IClientApi _clientApi;
        private readonly IJoinedRunApi _joinedRunApi;
        private readonly ILogger<RunController> _logger;

        public RunController(
            IRunApi runApi,
            IClientApi clientApi,
            IJoinedRunApi joinedRunApi,
            ILogger<RunController> logger)
        {
            _clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
            _joinedRunApi = joinedRunApi ?? throw new ArgumentNullException(nameof(joinedRunApi));
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
                    CurrentSortBy = sortBy
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
        public async Task<IActionResult> GetRunData(string id,  CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    return Json(new { success = false, message = "Run not found" });
                }

                var clientCourtList =  await _clientApi.GetClientCourtsAsync(run.ClientId, accessToken, cancellationToken);

                // Transform the data to match what the view expects
                var runData = new
                {
                    runId = run.RunId,
                    clientId = run.ClientId,
                    name = run.Name,
                    runDate = run.RunDate?.ToString("yyyy-MM-dd"),
                    startTime = run.StartTime.HasValue ? run.StartTime.Value.ToString(@"hh\:mm\:ss") : "", // Fallback to RunDate if StartTime is null
                    endTime = run.EndTime.HasValue ? run.EndTime.Value.ToString(@"hh\:mm\:ss") : "", // Default 2 hours if not specified
                    address = "Location not specified",
                    city = "t",
                    state = "t",
                    zip = "t",
                    playerLimit = run.PlayerLimit,
                    playerCount = run.PlayerCount ?? 0,
                    skillLevel = run.SkillLevel ?? "Intermediate",
                    description = run.Description ?? "",
                    type = run.Type ?? "Pickup",
                    status = run.Status ?? "Active",
                    isPublic = run.IsPublic ?? true,
                    teamType = run.TeamType ?? "Individual",
                    courtList = clientCourtList.Select(c => new
                    {
                        courtId = c.CourtId,
                        name = c.Name
                    }).ToList(),

                    success = true
                };

                return Json(runData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run data for ID: {RunId}", id);
                return Json(new { success = false, message = "Error loading run data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRunParticipants(string runId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Get participants for the run
                var participants = await _joinedRunApi.GetJoinedRunProfilesByRunIdAsync(runId, accessToken, cancellationToken);

                // Transform to expected format
                var participantData = participants.Select(p => new
                {
                    profileId = p.ProfileId,
                    userName = p.UserName ?? "Unknown Player",
                    imageUrl = p.ImageURL,
                    status = p.Status,
                    
                }).ToList();

                return Json(participantData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participants for run: {RunId}", runId);
                return Json(new List<object>());
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveParticipant([FromBody] string joinedRunId , string ProfileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Verify user has permission (is run creator or admin)
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                var run = await _runApi.GetRunByIdAsync(joinedRunId, accessToken, cancellationToken);
                if (run == null)
                {
                    return Json(new { success = false, message = "Run not found" });
                }

                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You do not have permission to remove participants from this run" });
                }

                await _joinedRunApi.RemoveProfileJoinRunAsync(joinedRunId, ProfileId, accessToken, cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant {ParticipantId} from run {RunId}", ProfileId, joinedRunId);
                return Json(new { success = false, message = "Error removing participant. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParticipant([FromBody] JoinedRun request, string runId,  string profileId , CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Verify user has permission (is run creator or admin)
                //var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                var run = await _runApi.GetRunByIdAsync(request.RunId, accessToken, cancellationToken);
                if (run == null)
                {
                    return Json(new { success = false, message = "Run not found" });
                }

                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You do not have permission to add participants to this run" });
                }

                // Check if run is at capacity
                if (run.PlayerCount >= run.PlayerLimit)
                {
                    return Json(new { success = false, message = "This run is already at maximum capacity" });
                }

                await _joinedRunApi.AddProfileToJoinedRunAsync(runId, profileId, accessToken, cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant to run {RunId}", request.RunId);
                return Json(new { success = false, message = "Error adding participant. Please try again." });
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

                // Create view model or return view with run data
                return View(run);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run details for ID: {RunId}", id);
                TempData["Error"] = "An error occurred while retrieving run details. Please try again later.";
                return RedirectToAction("Run");
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