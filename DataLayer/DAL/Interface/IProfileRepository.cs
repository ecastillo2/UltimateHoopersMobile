using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL
{
    /// <summary>
    /// Interface for profile repository operations with consistent cancellation token support
    /// </summary>
    public interface IProfileRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all profiles
        /// </summary>
        Task<List<Profile>> GetProfilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles with offset-based pagination
        /// </summary>
        Task<(List<Profile> Profiles, int TotalCount, int TotalPages)> GetProfilesPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Profiles and the next cursor value</returns>
        Task<(List<Profile> Profiles, string NextCursor)> GetProfilesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all profiles for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Profile> StreamAllProfilesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles that the specified profile is following
        /// </summary>
        Task<List<Profile>> GetFollowingProfilesAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles that follow the specified profile
        /// </summary>
        Task<List<Profile>> GetFollowerProfilesAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profile by ID
        /// </summary>
        Task<Profile> GetProfileByIdAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profile by user ID
        /// </summary>
        Task<Profile> GetProfileByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profile by username
        /// </summary>
        Task<Profile> GetProfileByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profile with timeout
        /// </summary>
        Task<Profile> GetProfileWithTimeoutAsync(
            string profileId,
            TimeSpan timeout,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get game history for a profile
        /// </summary>
        Task<List<Game>> GetProfileGameHistoryAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get game statistics for a profile
        /// </summary>
        Task<GameStatistics> GetProfileGameStatisticsAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get scouting report for a profile
        /// </summary>
        Task<ScoutingReport> GetScoutingReportAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get squad for a profile
        /// </summary>
        Task<Squad> GetProfileSquadAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get settings for a profile
        /// </summary>
        Task<Setting> GetProfileSettingsAsync(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task<bool> UpdateProfileAsync(
            Profile profile,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple profiles at once
        /// </summary>
        Task<int> BatchUpdateProfilesAsync(
            IEnumerable<Profile> profiles,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile points
        /// </summary>
        Task<bool> UpdateProfilePointsAsync(
            string profileId,
            int points,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Set top record status for a profile
        /// </summary>
        Task<bool> SetTopRecordStatusAsync(
            string profileId,
            bool hasTopRecord,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile's last run date
        /// </summary>
        Task<bool> UpdateLastRunDateAsync(
            string profileId,
            string lastRunDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile username
        /// </summary>
        Task<bool> UpdateProfileUserNameAsync(
            string profileId,
            string newUserName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile settings
        /// </summary>
        Task<bool> UpdateSettingAsync(
            Setting setting,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Upsert a scouting report
        /// </summary>
        Task<bool> UpsertScoutingReportAsync(
            ScoutingReport scoutingReport,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a username is available
        /// </summary>
        Task<bool> IsUserNameAvailableAsync(
            string userName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}