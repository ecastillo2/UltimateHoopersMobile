using Common.Utilities;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        public RunController(IRunApi runApi, IClientApi clientApi, IJoinedRunApi joinedRunApi, ILogger<RunController> logger)
        {
            _clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
            _joinedRunApi = joinedRunApi ?? throw new ArgumentNullException(nameof(joinedRunApi));
            _runApi = runApi ?? throw new ArgumentNullException(nameof(runApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Run(string cursor = null, int limit = 10, string direction = "next", string sortBy = "StartDate", CancellationToken cancellationToken = default)
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
                var result = await _runApi.GetRunsWithCursorAsync(cursor: cursor, limit: limit, direction: direction, sortBy: sortBy, accessToken: accessToken, cancellationToken: cancellationToken);

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

        // ENHANCED: Get runs for calendar view with better error handling
        [HttpGet]
        public async Task<IActionResult> GetRunsForCalendar(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Set default date range if not provided (current month +/- 2 months)
                var now = DateTime.Now;
                startDate ??= new DateTime(now.Year, now.Month, 1).AddMonths(-2);
                endDate ??= new DateTime(now.Year, now.Month, 1).AddMonths(3).AddDays(-1);

                _logger.LogInformation("Fetching runs for calendar from {StartDate} to {EndDate}", startDate, endDate);

                // Get runs within the date range
                var runs = await GetRunsForDateRange(startDate.Value, endDate.Value, accessToken, cancellationToken);

                // Transform runs for calendar display with proper data mapping and null safety
                var calendarRuns = runs.Select(run => new
                {
                    runId = run.RunId ?? "",
                    name = run.Client?.Name ?? run.Name ?? "Basketball Run", // ENHANCED: Use Client.Name as primary
                    type = run.Type?.ToLower() ?? "pickup",
                    runDate = run.RunDate?.ToString("yyyy-MM-dd") ?? "",
                    startTime = DateTimeUtilities.FormatTimeSpanTo12Hour(run.StartTime ?? TimeSpan.Zero),
                    endTime = DateTimeUtilities.FormatTimeSpanTo12Hour(run.EndTime ?? TimeSpan.Zero),
                    location = BuildLocationStringFixed(run),
                    skillLevel = run.SkillLevel ?? "All Levels",
                    playerCount = run.PlayerCount ?? 0,
                    playerLimit = run.PlayerLimit ?? 10,
                    description = run.Description ?? "",
                    status = run.Status ?? "Active",
                    isPublic = run.IsPublic ?? true,
                    address = run.Client?.Address ?? "",
                    city = run.Client?.City ?? "",
                    state = run.Client?.State ?? "",
                    profileId = run.ProfileId ?? ""
                }).ToList();

                _logger.LogInformation("Retrieved {Count} runs for calendar", calendarRuns.Count);

                return Json(new
                {
                    success = true,
                    runs = calendarRuns,
                    dateRange = new
                    {
                        startDate = startDate.Value.ToString("yyyy-MM-dd"),
                        endDate = endDate.Value.ToString("yyyy-MM-dd")
                    },
                    totalCount = calendarRuns.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runs for calendar");
                return Json(new
                {
                    success = false,
                    message = "Error loading calendar data",
                    error = ex.GetType().Name
                });
            }
        }

        private string BuildLocationStringFixed(Run run)
        {
            var locationParts = new List<string>();

            if (!string.IsNullOrEmpty(run.Client?.Address))
                locationParts.Add(run.Client.Address);
            if (!string.IsNullOrEmpty(run.Client?.State))
                locationParts.Add(run.Client.State);
            if (!string.IsNullOrEmpty(run.Client?.City) && locationParts.Any())
                locationParts.Add(run.Client.City);

            return locationParts.Any() ? string.Join(", ", locationParts) : "Location TBD";
        }

        // Helper method to get runs for a date range
        private async Task<List<Run>> GetRunsForDateRange(DateTime startDate, DateTime endDate, string accessToken, CancellationToken cancellationToken)
        {
            var allRuns = new List<Run>();
            string cursor = null;
            const int batchSize = 50;
            const int maxRuns = 500;

            try
            {
                do
                {
                    var result = await _runApi.GetRunsWithCursorAsync(
                        cursor: cursor,
                        limit: batchSize,
                        direction: "next",
                        sortBy: "RunDate",
                        accessToken: accessToken,
                        cancellationToken: cancellationToken);

                    if (result.Items?.Any() == true)
                    {
                        var filteredRuns = result.Items.Where(run =>
                            run.RunDate.HasValue &&
                            run.RunDate.Value.Date >= startDate.Date &&
                            run.RunDate.Value.Date <= endDate.Date).ToList();

                        allRuns.AddRange((IEnumerable<Run>)filteredRuns);

                        if (result.Items.Any(run => run.RunDate.HasValue && run.RunDate.Value.Date > endDate.Date))
                        {
                            break;
                        }
                    }

                    cursor = result.NextCursor;

                    if (allRuns.Count >= maxRuns)
                    {
                        _logger.LogWarning("Calendar query hit maximum run limit of {MaxRuns}", maxRuns);
                        break;
                    }

                } while (!string.IsNullOrEmpty(cursor));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRunsForDateRange");
                throw;
            }

            return allRuns.OrderBy(r => r.RunDate).ThenBy(r => r.StartTime).ToList();
        }

        // FIXED: GetRunData method with comprehensive null safety
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

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Run ID is required" });
                }

                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    return Json(new { success = false, message = "Run not found" });
                }

                var clientCourtList = new List<Court>();

                // Enhanced court data retrieval with comprehensive error handling
                try
                {
                    if (!string.IsNullOrEmpty(run.ClientId))
                    {
                        _logger.LogInformation("Fetching courts for client: {ClientId}", run.ClientId);
                        var courts = await _clientApi.GetClientCourtsAsync(run.ClientId, accessToken, cancellationToken);
                        clientCourtList = courts ?? new List<Court>();
                        _logger.LogInformation("Retrieved {CourtCount} courts for client {ClientId}",
                            clientCourtList.Count, run.ClientId);
                    }
                    else
                    {
                        _logger.LogWarning("No ClientId found for run {RunId}", run.RunId);
                    }
                }
                catch (Exception courtEx)
                {
                    _logger.LogError(courtEx, "Error retrieving courts for client: {ClientId}", run.ClientId);
                    // Continue execution even if court loading fails
                    clientCourtList = new List<Court>();
                }

                // Enhanced court list handling with comprehensive null safety
                var courtListData = new List<object>();
                if (clientCourtList != null && clientCourtList.Any())
                {
                    courtListData = clientCourtList.Select(c => new
                    {
                        courtId = c?.CourtId ?? "",
                        name = (c?.Name ?? "Unnamed Court") + " - " + (c?.CourtType ?? "Standard"),
                        address = c?.Address ?? "",
                        isIndoor = true,
                        isActive = true
                    }).Cast<object>().ToList();

                    _logger.LogInformation("Transformed {CourtCount} courts for API response", courtListData.Count);
                }
                else
                {
                    _logger.LogWarning("No courts available for client {ClientId}", run.ClientId);
                }

                // FIXED: Safe time formatting with comprehensive null checks
                string FormatTimeSafe(TimeSpan? timeSpan)
                {
                    if (!timeSpan.HasValue) return "";
                    try
                    {
                        return TimeSpan.FromTicks(timeSpan.Value.Ticks).ToString(@"hh\:mm");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error formatting time span: {TimeSpan}", timeSpan);
                        return "";
                    }
                }

                // ENHANCED: Run data response with comprehensive null safety and error handling
                var runData = new
                {
                    runId = run.RunId ?? "",
                    clientId = run.ClientId ?? "",
                    name = run.Name ?? "",

                    // FIXED: Client information with proper null checking
                    clientName = run.Client?.Name ?? "",
                    clientAddress = run.Client?.Address ?? "",
                    clientCity = run.Client?.City ?? "",
                    clientState = run.Client?.State ?? "",
                    clientZip = run.Client?.Zip ?? "",

                    // Safe date formatting
                    runDate = run.RunDate?.ToString("yyyy-MM-dd") ?? "",

                    // FIXED: Time formatting with comprehensive null safety
                    startTime = FormatTimeSafe(run.StartTime),
                    endTime = FormatTimeSafe(run.EndTime),

                    // FIXED: Address fields with proper null safety - avoiding duplicate access
                    address = run.Client?.Address ?? "",
                    city = run.Client?.City ?? "",
                    state = run.Client?.State ?? "",
                    zip = run.Client?.Zip ?? "",

                    // Safe numeric conversions with null coalescing
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
                    isOutdoor = false,

                    // Enhanced court selection with null safety
                    courtId = GetCourtIdFromRunSafe(run),

                    // Enhanced court list
                    courtList = courtListData,

                    success = true,

                    // ENHANCED: Default time suggestions based on run type
                    defaultTimes = GetDefaultTimesForRunType(run.Type),

                    // Additional debugging information
                    debug = new
                    {
                        hasClientId = !string.IsNullOrEmpty(run.ClientId),
                        clientId = run.ClientId ?? "",
                        courtCount = courtListData.Count,
                        runCourtId = GetCourtIdFromRunSafe(run),
                        hasClientInfo = run.Client != null,
                        clientName = run.Client?.Name ?? "No client data"
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
                    error = ex.GetType().Name,
                    runId = id ?? ""
                });
            }
        }

        // ENHANCED: Get default times based on run type
        private object GetDefaultTimesForRunType(string runType)
        {
            return runType?.ToLower() switch
            {
                "pickup" => new { startTime = "18:00", endTime = "20:00" }, // 6 PM - 8 PM
                "training" => new { startTime = "17:00", endTime = "19:00" }, // 5 PM - 7 PM  
                "tournament" => new { startTime = "09:00", endTime = "17:00" }, // 9 AM - 5 PM
                "youth" => new { startTime = "16:00", endTime = "17:30" }, // 4 PM - 5:30 PM
                "women" => new { startTime = "19:00", endTime = "21:00" }, // 7 PM - 9 PM
                _ => new { startTime = "18:00", endTime = "20:00" } // Default
            };
        }

        // FIXED: Enhanced GetClientData method with comprehensive null safety
        [HttpGet]
        public async Task<IActionResult> GetClientData(string clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                if (string.IsNullOrEmpty(clientId))
                {
                    return Json(new { success = false, message = "Client ID is required" });
                }

                // Get client data from API with comprehensive error handling
                var client = await _clientApi.GetClientByIdAsync(clientId, accessToken, cancellationToken);

                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }

                // Get courts for the client with comprehensive error handling
                List<Court> courts = new List<Court>();
                try
                {
                    var courtResult = await _clientApi.GetClientCourtsAsync(clientId, accessToken, cancellationToken);
                    courts = courtResult ?? new List<Court>(); // Ensure not null
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load courts for client {ClientId}", clientId);
                    // Continue without courts rather than failing entirely
                }

                var clientData = new
                {
                    success = true,
                    client = new
                    {
                        clientId = client.ClientId ?? "",
                        name = client.Name ?? "",
                        address = client.Address ?? "",
                        city = client.City ?? "",
                        state = client.State ?? "",
                        zip = client.Zip ?? "",
                        phoneNumber = client.PhoneNumber ?? ""
                    },
                    courts = courts.Select(c => new
                    {
                        courtId = c?.CourtId ?? "",
                        name = c?.Name ?? "Unnamed Court",
                    }) ?? Enumerable.Empty<object>()
                };

                return Json(clientData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client data for ID: {ClientId}", clientId);
                return Json(new
                {
                    success = false,
                    message = $"Error loading client data: {ex.Message}",
                    error = ex.GetType().Name
                });
            }
        }

        // FIXED: New method to get courts for a specific client with enhanced error handling
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

                List<Court> courts = new List<Court>();
                try
                {
                    var courtResult = await _clientApi.GetClientCourtsAsync(clientId, accessToken, cancellationToken);
                    courts = courtResult ?? new List<Court>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GetClientCourtsAsync returned error for client: {ClientId}", clientId);
                    // Continue with empty courts list
                }

                var courtData = courts.Select(c => new
                {
                    courtId = c?.CourtId ?? "",
                    name = c?.Name ?? "Unnamed Court",
                    address = c?.Address ?? "",
                    isIndoor = true,
                    isActive = true
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

        // FIXED: Helper method to safely get court ID from run data
        private string GetCourtIdFromRunSafe(dynamic run)
        {
            try
            {
                // Try multiple properties safely
                if (run?.CourtId != null)
                {
                    return run.CourtId.ToString();
                }

                if (run?.Court?.CourtId != null)
                {
                    return run.Court.CourtId.ToString();
                }

                if (run?.VenueId != null)
                {
                    return run.VenueId.ToString();
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

                if (string.IsNullOrEmpty(runId))
                {
                    return Json(new { success = false, message = "Run ID is required" });
                }

                var participants = await _joinedRunApi.GetJoinedRunProfilesByRunIdAsync(runId, accessToken, cancellationToken);

                var participantData = (participants ?? new List<Profile>()).Select(p => new
                {
                    profileId = p?.ProfileId ?? "",
                    userName = p?.UserName ?? "Unknown Player",
                    imageUrl = p?.ImageURL ?? "",
                    status = p?.Status ?? "Active"
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
                if (request == null || string.IsNullOrEmpty(request.RunId) || string.IsNullOrEmpty(request.ProfileId))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

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
                if (request == null || string.IsNullOrEmpty(request.RunId) || string.IsNullOrEmpty(request.ProfileId))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

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

                if ((run.PlayerCount ?? 0) >= (run.PlayerLimit ?? 0))
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
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view run details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Run ID is required.";
                    return RedirectToAction("Run");
                }

                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Run");
                }

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
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                if (run == null)
                {
                    TempData["Error"] = "Invalid run data provided.";
                    return View(new Run());
                }

                run.ProfileId = HttpContext.Session.GetString("ProfileId");

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

                var createdRun = await _runApi.CreateRunAsync(run, accessToken, cancellationToken);

                TempData["Success"] = "Run created successfully.";
                return RedirectToAction("Details", new { id = createdRun?.RunId });
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
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Run ID is required.";
                    return RedirectToAction("Run");
                }

                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Run");
                }

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

                if (run == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid run data provided."
                    });
                }

                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

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

                var existingRun = await _runApi.GetRunByIdAsync(run.RunId, accessToken, cancellationToken);
                if (existingRun == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Run not found. It may have been deleted by another user."
                    });
                }

                if (existingRun.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new
                    {
                        success = false,
                        message = "You do not have permission to edit this run."
                    });
                }

                if (run.PlayerLimit.HasValue && existingRun.PlayerCount.HasValue &&
                    run.PlayerLimit < existingRun.PlayerCount)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Cannot reduce player limit to {run.PlayerLimit} because there are already {existingRun.PlayerCount} participants joined."
                    });
                }

                SetDefaultRunValues(run, existingRun);

                run.ProfileId = existingRun.ProfileId;

                if (!run.PlayerCount.HasValue)
                {
                    run.PlayerCount = existingRun.PlayerCount;
                }

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
                        address = refreshedRun.Client?.Address ?? "",
                        city = refreshedRun.Client?.City ?? "",
                        state = refreshedRun.Client?.State ?? "",
                        zip = refreshedRun.Client?.Zip ?? ""
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
                        address = run.Client?.Address ?? "",
                        city = run.Client?.City ?? "",
                        state = run.Client?.State ?? "",
                        zip = run.Client?.Zip ?? ""
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
                    error = ex.GetType().Name
                });
            }
        }

        private ValidationResult ValidateRunData(Run run)
        {
            if (run == null)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Run data is required.", Field = "run" };
            }

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
            if (run == null || existingRun == null) return;

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

            run.CreatedDate = existingRun.CreatedDate;
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
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to delete a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Run ID is required.";
                    return RedirectToAction("Run");
                }

                var run = await _runApi.GetRunByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Run");
                }

                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to delete this run.";
                    return RedirectToAction("Details", new { id = id });
                }

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
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to join a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                if (string.IsNullOrEmpty(runId))
                {
                    TempData["Error"] = "Run ID is required.";
                    return RedirectToAction("Run");
                }

                var profileId = HttpContext.Session.GetString("ProfileId");
                if (string.IsNullOrEmpty(profileId))
                {
                    TempData["Error"] = "User profile not found.";
                    return RedirectToAction("Details", new { id = runId });
                }

                var joinedRun = new CreateJoinedRunDto
                {
                    RunId = runId,
                    ProfileId = profileId
                };

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