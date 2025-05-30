using Domain;
using Domain.DtoModel;

namespace WebAPI.ApiClients
{
    public interface IProfileApi
    {
        /// <summary>
        /// Deletes a profile by its ID
        /// </summary>
        /// <param name="profileId">The ID of the profile to delete</param>
        /// <param name="accessToken">Authorization token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteProfileAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a profile by its ID
        /// </summary>
        /// <param name="profileId">The ID of the profile to retrieve</param>
        /// <param name="accessToken">Authorization token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The retrieved profile</returns>
        Task<Profile> GetProfileByIdAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a profile
        /// </summary>
        /// <param name="profile">The profile with updated information</param>
        /// <param name="accessToken">Authorization token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateProfileAsync(Profile profile, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a profile
        /// </summary>
        /// <param name="profile">The profile with updated information</param>
        /// <param name="accessToken">Authorization token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateScoutingReportAsync(ScoutingReport scoutingReport, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of all profiles
        /// </summary>
        /// <param name="accessToken">Authorization token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of profiles</returns>
        Task<List<Profile>> GetProfilesAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of profiles using cursor-based pagination
        /// </summary>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Number of items per page</param>
        /// <param name="direction">Direction of pagination (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="accessToken">Authorization token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated result of profiles</returns>
        Task<CursorPaginatedResultDto<ProfileViewModelDto>> GetProfilesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);
    }
}