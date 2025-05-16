using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL
{
    /// <summary>
    /// Interface for profile repository operations with cancellation token support
    /// </summary>
    public interface IProfileRepository : IDisposable
    {
        /// <summary>
        /// Get all profiles
        /// </summary>
        Task<List<Profile>> GetProfiles(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles with pagination
        /// </summary>
        Task<(List<Profile> Profiles, int TotalCount, int TotalPages)> GetProfilesPaginated(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all profiles for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Profile> StreamAllProfiles(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles that the specified profile is following
        /// </summary>
        Task<List<Profile>> GetFollowingProfilesByProfileId(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles that follow the specified profile
        /// </summary>
        Task<List<Profile>> GetFollowerProfilesByProfileId(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profile by ID
        /// </summary>
        Task<Profile> GetProfileById(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profile by ID with timeout
        /// </summary>
        Task<Profile> GetProfileByIdWithTimeout(
            string profileId,
            TimeSpan timeout,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get game history for a profile
        /// </summary>
        Task<List<Game>> GetProfileGameHistory(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task UpdateProfile(
            Profile model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple profiles at once
        /// </summary>
        Task BatchUpdateProfiles(
            List<Profile> profiles,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update winner points
        /// </summary>
        Task UpdateWinnerPoints(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update top record flag to true
        /// </summary>
        Task UpdateSetProfileWithBestRecord(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update top record flag to false
        /// </summary>
        Task UpdateSetProfileWithBestRecordToFalse(
            string profileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile's last run date
        /// </summary>
        Task UpdateLastRunDate(
            string profileId,
            string lastRunDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile username
        /// </summary>
        Task UpdateProfileUserName(
            Profile model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile settings
        /// </summary>
        Task UpdateSetting(
            Setting model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a username is available
        /// </summary>
        Task<bool> IsUserNameAvailable(
            string userName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if an email is available
        /// </summary>
        Task<bool> IsEmailAvailable(
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> Save(CancellationToken cancellationToken = default);

        // Legacy methods without cancellation token for backward compatibility
        Task<List<Profile>> GetProfiles();
        Task<List<Profile>> GetFollowingProfilesByProfileId(string profileId);
        Task<List<Profile>> GetFollowerProfilesByProfileId(string profileId);
        Task<Profile> GetProfileById(string profileId);
        Task<List<Game>> GetProfileGameHistory(string profileId);
        Task UpdateProfile(Profile model);
        Task UpdateWinnerPoints(string profileId);
        Task UpdateSetProfileWithBestRecord(string profileId);
        Task UpdateSetProfileWithBestRecordToFalse(string profileId);
        Task UpdateLastRunDate(string profileId, string lastRunDate);
        Task UpdateProfileUserName(Profile model);
        Task UpdateSetting(Setting model);
        Task<bool> IsUserNameAvailable(string userName);
        Task<bool> IsEmailAvailable(string email);
        Task<int> Save();
    }
}