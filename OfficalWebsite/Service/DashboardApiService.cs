using ApiClient.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.DtoModel;
using ApiClient.Configuration;

namespace UltimateHoopers.Services
{
    public class DashboardApiService : BaseApiClient
    {
        private readonly string _baseUrl;
        private readonly ILogger<DashboardApiService> _logger;

        public DashboardApiService(
            HttpClient httpClient,
            IOptions<ApiClientOptions> options,
            ILogger<DashboardApiService> logger)
            : base(httpClient, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options == null || options.Value == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _baseUrl = options.Value.BaseUrl;
            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new ArgumentException("API base URL is not configured", nameof(options));
            }

            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
        }

        /// <summary>
        /// Gets dashboard statistics for a user
        /// </summary>
        /// <param name="profileId">The profile ID</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dashboard statistics</returns>
        public async Task<DashboardStats> GetDashboardStatsAsync(
            string profileId,
            string token,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetAsync<DashboardStats>(
                    $"api/Dashboard/GetStats?profileId={profileId}",
                    token,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats for profile {ProfileId}", profileId);
                throw new ApiException("Failed to retrieve dashboard statistics", ex);
            }
        }

        /// <summary>
        /// Gets recent activity for a user
        /// </summary>
        /// <param name="profileId">The profile ID</param>
        /// <param name="token">Authentication token</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of recent activity items</returns>
        public async Task<List<ActivityItem>> GetRecentActivityAsync(
            string profileId,
            string token,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetAsync<List<ActivityItem>>(
                    $"api/Dashboard/GetRecentActivity?profileId={profileId}&limit={limit}",
                    token,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity for profile {ProfileId}", profileId);
                throw new ApiException("Failed to retrieve recent activity", ex);
            }
        }

        /// <summary>
        /// Gets nearby courts for a user
        /// </summary>
        /// <param name="profileId">The profile ID</param>
        /// <param name="token">Authentication token</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of nearby courts</returns>
        public async Task<List<CourtViewModelDto>> GetNearbyCourtsAsync(
            string profileId,
            string token,
            int limit = 3,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetAsync<List<CourtViewModelDto>>(
                    $"api/Dashboard/GetNearbyCourts?profileId={profileId}&limit={limit}",
                    token,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby courts for profile {ProfileId}", profileId);
                throw new ApiException("Failed to retrieve nearby courts", ex);
            }
        }

        /// <summary>
        /// Gets player leaderboard
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of leaderboard items</returns>
        public async Task<List<LeaderboardItem>> GetLeaderboardAsync(
            string token,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetAsync<List<LeaderboardItem>>(
                    $"api/Dashboard/GetLeaderboard?limit={limit}",
                    token,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard data");
                throw new ApiException("Failed to retrieve leaderboard data", ex);
            }
        }
    }

    // Model classes

    public class DashboardStats
    {
        public int WinRate { get; set; }
        public float Rating { get; set; }
        public int TotalRuns { get; set; }
        public int TotalGames { get; set; }
        public int TotalPoints { get; set; }
        public int SquadMembers { get; set; }
    }

    public class ActivityItem
    {
        public string ActivityId { get; set; }
        public string ActivityType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string RelativeTime { get; set; }
    }

    public class LeaderboardItem
    {
        public int Rank { get; set; }
        public string ProfileId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public float Rating { get; set; }
    }
}