using Common.Utilities;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

                // Enhanced court data retrieval with better error handling
                try
                {
                    if (!string.IsNullOrEmpty(run.ClientId))
                    {
                        _logger.LogInformation("Fetching courts for client: {ClientId}", run.ClientId);
                        clientCourtList = await _clientApi.GetClientCourtsAsync(run.ClientId, accessToken, cancellationToken);
                        _logger.LogInformation("Retrieved {CourtCount} courts for client {ClientId}",
                            clientCourtList?.Count ?? 0, run.ClientId);
                    }
                    else
                    {
                        _logger.LogWarning("No ClientId found for run {RunId}", run.RunId);
                    }
                }
                catch (Exception courtEx)
                {
                    _logger.LogError(courtEx, "Error retrieving courts for client: {ClientId}", run.ClientId);
                    // Continue with empty courts list
                }

                // Enhanced court list handling with better structure
                var courtListData = new List<object>();
                if (clientCourtList != null && clientCourtList.Any())
                {
                    courtListData = clientCourtList.Select(c => new
                    {
                        courtId = c.CourtId ?? "",
                        name = c.Name + " - "+ c.CourtType ?? "Unnamed Court",
                        // Add additional court properties if needed
                        address = c.Address ?? "",
                        isIndoor =  true,
                        isActive =  true
                    }).Cast<object>().ToList();

                    _logger.LogInformation("Transformed {CourtCount} courts for API response", courtListData.Count);
                }
                else
                {
                    _logger.LogWarning("No courts available for client {ClientId}", run.ClientId);
                }

                // Enhanced run data response with comprehensive court information
                var runData = new
                {
                    runId = run.RunId ?? "",
                    clientId = run.ClientId ?? "",
                    name = run.Name ?? "",

                    // Safe date formatting
                    runDate = run.RunDate?.ToString("yyyy-MM-dd") ?? "",

                    // Safe TimeSpan formatting
                    startTime = DateTimeUtilities.FormatTimeSpanTo12Hour(run.StartTime ?? TimeSpan.Zero),
                    endTime = DateTimeUtilities.FormatTimeSpanTo12Hour(run.EndTime ?? TimeSpan.Zero),

                    // Address fields with defaults
                    address =  "",
                    city =  "",
                    state =  "",
                    zip =  "",

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
                    isOutdoor =  false,

                    // Enhanced court selection
                    courtId = GetCourtIdFromRun(run),

                    // Enhanced court list with comprehensive data
                    courtList = courtListData,

                    success = true,

                    // Additional debugging information (remove in production)
                    debug = new
                    {
                        hasClientId = !string.IsNullOrEmpty(run.ClientId),
                        clientId = run.ClientId,
                        courtCount = courtListData.Count,
                        runCourtId = GetCourtIdFromRun(run)
                    }
                };

                _logger.LogInformation("Successfully prepared run data for {RunId} with {CourtCount} courts",
                    run.RunId, courtListData.Count);

                return Json(runData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run data for ID: {RunId}", id);
                return Json(new
                {
                    success = false,
                    message = $"Error loading run data: {ex.Message}",
                    error = ex.GetType().Name // Additional debugging info
                });
            }
        }


        // New method to get courts for a specific client
        [HttpGet]
        public async Task<IActionResult> GetRunCourts(string clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (string.IsNullOrEmpty(clientId))
                {
                    return Json(new { success = true, courts = new List<object>() });
                }

                _logger.LogInformation("Fetching courts for client: {ClientId}", clientId);

                // Get courts for the client
                var courts = await _clientApi.GetClientCourtsAsync(clientId, accessToken, cancellationToken);

                if (courts == null)
                {
                    _logger.LogWarning("GetClientCourtsAsync returned null for client: {ClientId}", clientId);
                    courts = new List<Court>();
                }

                // Transform courts data with comprehensive information
                var courtData = courts.Select(c => new
                {
                    courtId = c.CourtId ?? "",
                    name = c.Name ?? "Unnamed Court",
                    address = c.Address ?? "",
                    isIndoor = true,
                    isActive =  true
                }).ToList();

                _logger.LogInformation("Successfully retrieved {CourtCount} courts for client {ClientId}",
                    courtData.Count, clientId);

                return Json(new
                {
                    success = true,
                    courts = courtData,
                    clientId = clientId,
                    count = courtData.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courts for client: {ClientId}", clientId);
                return Json(new
                {
                    success = false,
                    message = "Error loading courts",
                    error = ex.GetType().Name
                });
            }
        }

       

        // Helper method to safely get court ID from run data
        private string GetCourtIdFromRun(dynamic run)
        {
            try
            {
                // Try different possible property names for court ID
                if (run.CourtId != null)
                {
                    var courtId = run.CourtId.ToString();
                   // _logger.LogDebug("Found CourtId: {CourtId}", courtId);
                    return courtId;
                }

                if (run.Court?.CourtId != null)
                {
                    var courtId = run.Court.CourtId.ToString();
                  //  _logger.LogWarning("Found Court.CourtId: {CourtId}", courtId);
                    return courtId;
                }

                if (run.VenueId != null)
                {
                    var venueId = run.VenueId.ToString();
                   // _logger.LogDebug("Found VenueId (using as CourtId): {VenueId}", venueId);
                    return venueId;
                }

                _logger.LogDebug("No court ID found in run data");
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting court ID from run data");
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
                    return Json(new
                    {
                        success = false,
                        message = "You must be logged in to edit a run.",
                        requiresLogin = true
                    });
                }

                // Get user info for permission checking
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Validate the run data
                var validationResult = ValidateRunData(run);
                if (!validationResult.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = validationResult.ErrorMessage,
                        field = validationResult.Field
                    });
                }

                // Get the existing run to check permissions
                var existingRun = await _runApi.GetRunByIdAsync(run.RunId, accessToken, cancellationToken);
                if (existingRun == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Run not found. It may have been deleted by another user."
                    });
                }

                // Verify user has permission to edit this run
                if (existingRun.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new
                    {
                        success = false,
                        message = "You do not have permission to edit this run."
                    });
                }

                // Check if player limit is being reduced below current participant count
                if (run.PlayerLimit.HasValue && existingRun.PlayerCount.HasValue &&
                    run.PlayerLimit < existingRun.PlayerCount)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Cannot reduce player limit to {run.PlayerLimit} because there are already {existingRun.PlayerCount} participants joined."
                    });
                }

                // Set default values if not provided
                SetDefaultRunValues(run, existingRun);

                // Preserve the original ProfileId and other system fields
                run.ProfileId = existingRun.ProfileId;

                // Preserve player count if not provided
                if (!run.PlayerCount.HasValue)
                {
                    run.PlayerCount = existingRun.PlayerCount;
                }

                // Update run via API
                var updateResult = await _runApi.UpdateRunAsync(run, accessToken, cancellationToken);

                if (!updateResult)
                {
                    _logger.LogWarning("Failed to update run {RunId} via API", run.RunId);
                    return Json(new
                    {
                        success = false,
                        message = "Failed to update the run. Please try again later."
                    });
                }

                _logger.LogInformation("Run updated successfully by user {ProfileId}: {RunId}", profileId, run.RunId);

                // Fetch the updated run data to return current state
                var refreshedRun = await _runApi.GetRunByIdAsync(run.RunId, accessToken, cancellationToken);

                return Json(new
                {
                    success = true,
                    message = "Run updated successfully!",
                    runId = run.RunId,
                    updatedRun = refreshedRun != null ? new
                    {
                        runId = refreshedRun.RunId,
                        name = refreshedRun.Name,
                        runDate = refreshedRun.RunDate?.ToString("yyyy-MM-dd"),
                        startTime = DateTimeUtilities.FormatTimeSpanTo12Hour(refreshedRun.StartTime ?? TimeSpan.Zero),
                        endTime = DateTimeUtilities.FormatTimeSpanTo12Hour(refreshedRun.EndTime ?? TimeSpan.Zero),
                        playerLimit = refreshedRun.PlayerLimit,
                        playerCount = refreshedRun.PlayerCount,
                        status = refreshedRun.Status,
                        skillLevel = refreshedRun.SkillLevel,
                        type = refreshedRun.Type,
                        isPublic = refreshedRun.IsPublic,
                        address = "",
                        city = "",
                        state = "",
                        zip = ""
                    } : new
                    {
                        runId = run.RunId,
                        name = run.Name,
                        runDate = run.RunDate?.ToString("yyyy-MM-dd"),
                        startTime = DateTimeUtilities.FormatTimeSpanTo12Hour(run.StartTime ?? TimeSpan.Zero),
                        endTime = DateTimeUtilities.FormatTimeSpanTo12Hour(run.EndTime ?? TimeSpan.Zero),
                        playerLimit = run.PlayerLimit,
                        playerCount = run.PlayerCount,
                        status = run.Status,
                        skillLevel = run.SkillLevel,
                        type = run.Type,
                        isPublic = run.IsPublic,
                        address = "",
                        city = "",
                        state = "",
                        zip = ""
                    }
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized attempt to update run {RunId} by user {ProfileId}", run?.RunId, HttpContext.Session.GetString("ProfileId"));

                return Json(new
                {
                    success = false,
                    message = "Your session has expired. Please log in again.",
                    requiresLogin = true
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data provided for run update: {RunId}", run?.RunId);

                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API communication error while updating run: {RunId}", run?.RunId);

                return Json(new
                {
                    success = false,
                    message = "Unable to connect to the server. Please check your internet connection and try again."
                });
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout while updating run: {RunId}", run?.RunId);

                return Json(new
                {
                    success = false,
                    message = "The request timed out. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating run: {RunId}", run?.RunId);

                return Json(new
                {
                    success = false,
                    message = "An unexpected error occurred while updating the run. Please try again later.",
                    error = ex.GetType().Name // Only include in development
                });
            }
        }


        private ValidationResult ValidateRunData(Run run)
        {
            if (string.IsNullOrEmpty(run.RunId))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run ID is required.", Field = "runId" };
            }

            if (string.IsNullOrEmpty(run.Name?.Trim()))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run name is required.", Field = "name" };
            }

            if (run.Name.Length > 100)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run name cannot exceed 100 characters.", Field = "name" };
            }

            if (string.IsNullOrEmpty(run.Description?.Trim()))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run description is required.", Field = "description" };
            }

            if (run.Description.Length > 1000)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run description cannot exceed 1000 characters.", Field = "description" };
            }

            if (!run.RunDate.HasValue)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run date is required.", Field = "runDate" };
            }

            if (run.RunDate.Value.Date < DateTime.Today)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run date cannot be in the past.", Field = "runDate" };
            }

            if (!run.StartTime.HasValue)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Start time is required.", Field = "startTime" };
            }

            if (run.EndTime.HasValue && run.StartTime.HasValue && run.EndTime <= run.StartTime)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "End time must be after start time.", Field = "endTime" };
            }

            if (!run.PlayerLimit.HasValue || run.PlayerLimit <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Player limit must be greater than 0.", Field = "playerLimit" };
            }

            if (run.PlayerLimit > 50)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Player limit cannot exceed 50.", Field = "playerLimit" };
            }

            return new ValidationResult { IsValid = true };
        }

        private void SetDefaultRunValues(Run run, Run existingRun)
        {
            if (string.IsNullOrEmpty(run.Status))
            {
                run.Status = existingRun.Status ?? "Active";
            }

            if (string.IsNullOrEmpty(run.Type))
            {
                run.Type = existingRun.Type ?? "Pickup";
            }

            if (string.IsNullOrEmpty(run.SkillLevel))
            {
                run.SkillLevel = existingRun.SkillLevel ?? "Intermediate";
            }

            if (string.IsNullOrEmpty(run.TeamType))
            {
                run.TeamType = existingRun.TeamType ?? "Individual";
            }

            if (!run.IsPublic.HasValue)
            {
                run.IsPublic = existingRun.IsPublic ?? true;
            }

            // Preserve creation metadata
            run.CreatedDate = existingRun.CreatedDate;
           // run.CreatedBy = existingRun.CreatedBy;

            // Set update metadata
           // run.UpdatedDate = DateTime.UtcNow;
            //run.UpdatedBy = HttpContext.Session.GetString("ProfileId");
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
            public string Field { get; set; }
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

   
