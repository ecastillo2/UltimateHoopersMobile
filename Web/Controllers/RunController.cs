using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.ApiClients;
using Website.Attributes;
using Website.ViewModels;

namespace Web.Controllers
{
    [Authentication] // Require authentication for all actions in this controller
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

        // Fixed GetRunData method in RunController.cs
        // Replace the existing GetRunData method in Web/Controllers/RunController.cs with this implementation
        [HttpGet]
        public async Task<IActionResult> GetRunData(string id, CancellationToken cancellationToken = default)
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

                var clientCourtList = new List<Court>();

                // Safely handle court data retrieval
                try
                {
                    if (!string.IsNullOrEmpty(run.ClientId))
                    {
                        clientCourtList = await _clientApi.GetClientCourtsAsync(run.ClientId, accessToken, cancellationToken);
                    }
                }
                catch (Exception courtEx)
                {
                    _logger.LogError(courtEx, "Error retrieving courts for client: {ClientId}", run.ClientId);
                    // Continue with empty courts list
                }

                // Safe court list handling
                var courtListData = new List<object>();
                if (clientCourtList != null && clientCourtList.Any())
                {
                    courtListData = clientCourtList.Select(c => new
                    {
                        courtId = c.CourtId ?? "",
                        name = c.Name ?? "Unnamed Court"
                    }).Cast<object>().ToList();
                }

                // Transform the data to match what the view expects with safe conversions
                var runData = new
                {
                    runId = run.RunId ?? "",
                    clientId = run.ClientId ?? "",
                    name = run.Name ?? "",

                    // Safe date formatting
                    runDate = run.RunDate?.ToString("yyyy-MM-dd") ?? "",

                    // Safe TimeSpan formatting - handle both TimeSpan and nullable TimeSpan
                    startTime = FormatTimeSpan(run.StartTime),
                    endTime = FormatTimeSpan(run.EndTime),

                    // Default address fields - these seem to be hardcoded in original
                    address ="Location not specified",
                    city = "Not specified",
                    state =  "Not specified",
                    zip = "Not specified",

                    // Safe numeric conversions
                    playerLimit = run.PlayerLimit ?? 0,
                    playerCount = run.PlayerCount ?? 0,

                    // Safe string assignments with defaults
                    skillLevel = run.SkillLevel ?? "Intermediate",
                    description = run.Description ?? "",
                    type = run.Type ?? "Pickup",
                    status = run.Status ?? "Active",
                    teamType = run.TeamType ?? "Individual",

                    // Safe boolean conversion
                    isPublic = run.IsPublic ?? true,

                    // Safe court selection - get the court ID if available
                    courtId = GetCourtIdFromRun(run),

                    courtList = courtListData,

                    success = true
                };

                return Json(runData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run data for ID: {RunId}", id);
                return Json(new { success = false, message = $"Error loading run data: {ex.Message}" });
            }
        }

        // Helper method to safely format TimeSpan values
        private string FormatTimeSpan(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue)
                return "";

            try
            {
                // Format as HH:mm for HTML time input
                return timeSpan.Value.ToString(@"hh\:mm");
            }
            catch (Exception)
            {
                return "";
            }
        }

        // Helper method to safely get court ID from run data
        private string GetCourtIdFromRun(dynamic run)
        {
            try
            {
                // Try different possible property names for court ID
                if (run.CourtId != null)
                    return run.CourtId.ToString();
                if (run.Court?.CourtId != null)
                    return run.Court.CourtId.ToString();
                if (run.VenueId != null)
                    return run.VenueId.ToString();

                return "";
            }
            catch (Exception)
            {
                return "";
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
                    status = p.Status ?? "Active"
                }).ToList();

                return Json(participantData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participants for run: {RunId}", runId);
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveParticipant([FromBody] RemoveParticipantRequest request, CancellationToken cancellationToken = default)
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

                var run = await _runApi.GetRunByIdAsync(request.RunId, accessToken, cancellationToken);
                if (run == null)
                {
                    return Json(new { success = false, message = "Run not found" });
                }

                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You do not have permission to remove participants from this run" });
                }

                await _joinedRunApi.RemoveProfileJoinRunAsync(request.RunId, request.ProfileId, accessToken, cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant {ParticipantId} from run {RunId}", request?.ProfileId, request?.RunId);
                return Json(new { success = false, message = "Error removing participant. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParticipant([FromBody] AddParticipantRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Verify user has permission (is run creator or admin)
                var currentProfileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                var run = await _runApi.GetRunByIdAsync(request.RunId, accessToken, cancellationToken);
                if (run == null)
                {
                    return Json(new { success = false, message = "Run not found" });
                }

                if (run.ProfileId != currentProfileId && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You do not have permission to add participants to this run" });
                }

                // Check if run is at capacity
                if (run.PlayerCount >= run.PlayerLimit)
                {
                    return Json(new { success = false, message = "This run is already at maximum capacity" });
                }

                await _joinedRunApi.AddProfileToJoinedRunAsync(request.RunId, request.ProfileId, accessToken, cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant to run {RunId}", request?.RunId);
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
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Run");
                }

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

                // Set default values if not provided
                if (run.PlayerLimit == null || run.PlayerLimit <= 0)
                {
                    run.PlayerLimit = 10;
                }

                if (string.IsNullOrEmpty(run.Status))
                {
                    run.Status = "Active";
                }

                if (string.IsNullOrEmpty(run.Type))
                {
                    run.Type = "Pickup";
                }

                if (string.IsNullOrEmpty(run.SkillLevel))
                {
                    run.SkillLevel = "Intermediate";
                }

                if (run.IsPublic == null)
                {
                    run.IsPublic = true;
                }

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
                    return RedirectToAction("Run");
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
                return RedirectToAction("Run");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] Run run, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "You must be logged in to edit a run." });
                }

                // Get user info for permission checking
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Validate the run data
                if (string.IsNullOrEmpty(run.RunId))
                {
                    return Json(new { success = false, message = "Run ID is required." });
                }

                if (string.IsNullOrEmpty(run.Name))
                {
                    return Json(new { success = false, message = "Run name is required." });
                }

                if (string.IsNullOrEmpty(run.Description))
                {
                    return Json(new { success = false, message = "Run description is required." });
                }

                if (!run.RunDate.HasValue)
                {
                    return Json(new { success = false, message = "Run date is required." });
                }

                if (!run.StartTime.HasValue)
                {
                    return Json(new { success = false, message = "Start time is required." });
                }

                if (!run.PlayerLimit.HasValue || run.PlayerLimit <= 0)
                {
                    return Json(new { success = false, message = "Player limit must be greater than 0." });
                }

                // Get the existing run to check permissions
                var existingRun = await _runApi.GetRunByIdAsync(run.RunId, accessToken, cancellationToken);
                if (existingRun == null)
                {
                    return Json(new { success = false, message = "Run not found." });
                }

                // Verify user has permission to edit this run
                if (existingRun.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You do not have permission to edit this run." });
                }

                // Set default values if not provided
                if (string.IsNullOrEmpty(run.Status))
                {
                    run.Status = "Active";
                }

                if (string.IsNullOrEmpty(run.Type))
                {
                    run.Type = "Pickup";
                }

                if (string.IsNullOrEmpty(run.SkillLevel))
                {
                    run.SkillLevel = "Intermediate";
                }

                if (string.IsNullOrEmpty(run.TeamType))
                {
                    run.TeamType = "Individual";
                }

                if (!run.IsPublic.HasValue)
                {
                    run.IsPublic = true;
                }

                // Preserve the original ProfileId and other system fields
                run.ProfileId = existingRun.ProfileId;

                // Preserve player count if not provided
                if (!run.PlayerCount.HasValue)
                {
                    run.PlayerCount = existingRun.PlayerCount;
                }

                // Update run via API
                await _runApi.UpdateRunAsync(run, accessToken, cancellationToken);

                _logger.LogInformation("Run updated successfully by user {ProfileId}: {RunId}", profileId, run.RunId);

                return Json(new
                {
                    success = true,
                    message = "Run updated successfully.",
                    runId = run.RunId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating run: {RunId}", run?.RunId);

                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the run. Please try again later."
                });
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
                    return RedirectToAction("Run");
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
                return RedirectToAction("Run");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting run: {RunId}", id);
                TempData["Error"] = "An error occurred while deleting the run. Please try again later.";
                return RedirectToAction("Run");
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

    // Request models for API calls
    public class RemoveParticipantRequest
    {
        public string RunId { get; set; }
        public string ProfileId { get; set; }
    }

    public class AddParticipantRequest
    {
        public string RunId { get; set; }
        public string ProfileId { get; set; }
    }
}