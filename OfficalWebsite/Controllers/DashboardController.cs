using Microsoft.AspNetCore.Mvc;
using ApiClient.Core;
using ApiClient.Authentication;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Domain;
using System.Threading;
using System;
using System.Collections.Generic;
using WebAPI.ApiClients;
using Domain.DtoModel;
using System.Linq;

namespace UltimateHoopers.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAuthenticateUser _authService;
        private readonly IRunApi _runApi;
        private readonly IProfileApi _profileApi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IAuthenticateUser authService,
            IRunApi runApi,
            IProfileApi profileApi,
            IConfiguration configuration,
            ILogger<DashboardController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _runApi = runApi ?? throw new ArgumentNullException(nameof(runApi));
            _profileApi = profileApi ?? throw new ArgumentNullException(nameof(profileApi));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Check if user is authenticated
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get user profile ID from session
                var profileId = HttpContext.Session.GetString("ProfileId");
                if (string.IsNullOrEmpty(profileId))
                {
                    _logger.LogWarning("Profile ID not found in session");
                    return RedirectToAction("Login", "Account");
                }

                // Create a dashboard view model
                var dashboardViewModel = new DashboardViewModel();

                // Get user profile
                var profile = await _profileApi.GetProfileByIdAsync(profileId, token);
                if (profile != null)
                {
                    dashboardViewModel.Profile = profile;
                }

                // Get upcoming runs
                var runsResult = await _runApi.GetRunsWithCursorAsync(
                    cursor: null,
                    limit: 5,
                    direction: "next",
                    sortBy: "RunDate",
                    accessToken: token);

                if (runsResult != null)
                {
                    dashboardViewModel.UpcomingRuns = runsResult.Items
                        .Where(r => r.RunDate > DateTime.Now)
                        .OrderBy(r => r.RunDate)
                        .ToList();
                }

                // Get player statistics
                dashboardViewModel.WinRate = 76; // Placeholder, replace with actual data
                dashboardViewModel.Rating = 4.8f; // Placeholder, replace with actual data
                dashboardViewModel.TotalRuns = 24; // Placeholder, replace with actual data
                dashboardViewModel.TotalPoints = 850; // Placeholder, replace with actual data

                // Get player leaderboard
                dashboardViewModel.Leaderboard = await GetLeaderboardAsync(token);

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View("Error", new ErrorViewModel { Message = "An error occurred while loading the dashboard. Please try again later." });
            }
        }

        private async Task<List<LeaderboardItem>> GetLeaderboardAsync(string token)
        {
            try
            {
                // Get top players based on points (this is a placeholder)
                // In a real implementation, you would have a specific API endpoint for this
                var profiles = await _profileApi.GetProfilesWithCursorAsync(
                    cursor: null,
                    limit: 5,
                    direction: "next",
                    sortBy: "Points",
                    accessToken: token);

                if (profiles == null || profiles.Items == null)
                {
                    return new List<LeaderboardItem>();
                }

                // Convert to leaderboard items
                var leaderboard = profiles.Items.Select((profile, index) => new LeaderboardItem
                {
                    Rank = index + 1,
                    ProfileId = profile.ProfileId,
                    Name = profile.UserName ?? "Unknown",
                    Points = profile.Points ?? 0,
                    Rating = float.TryParse(profile.StarRating, out float rating) ? rating : 0
                }).ToList();

                return leaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard data");
                return new List<LeaderboardItem>();
            }
        }
    }

    public class DashboardViewModel
    {
        public Profile Profile { get; set; }
        public List<RunDetailViewModelDto> UpcomingRuns { get; set; } = new List<RunDetailViewModelDto>();
        public List<LeaderboardItem> Leaderboard { get; set; } = new List<LeaderboardItem>();
        public int WinRate { get; set; }
        public float Rating { get; set; }
        public int TotalRuns { get; set; }
        public int TotalPoints { get; set; }
    }

    public class LeaderboardItem
    {
        public int Rank { get; set; }
        public string ProfileId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public float Rating { get; set; }
    }

    public class ErrorViewModel
    {
        public string Message { get; set; }
    }
}