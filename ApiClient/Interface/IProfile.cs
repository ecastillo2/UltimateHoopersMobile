using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Profile API operations
    /// </summary>
    public interface IProfileApi
    {
        /// <summary>
        /// Get all Profiles
        /// </summary>
        Task<List<Profile>> GetProfilesAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets profiles with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">Optional cursor from a previous result set to continue pagination</param>
        /// <param name="limit">Number of items to return per request</param>
        /// <param name="direction">Direction of pagination: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by: "Points", "PlayerNumber", "Username", or "Status"</param>
        /// <param name="accessToken">Optional authentication token</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>A cursor-paginated result containing profiles and pagination metadata</returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails</exception>
        /// <exception cref="JsonException">Thrown when deserialization fails</exception>
        Task<CursorPaginatedResultDto<ProfileViewModelDto>> GetProfilesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Profile by ID
        /// </summary>
        Task<Profile> GetProfileByIdAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing Profile
        /// </summary>
        Task<bool> UpdateProfileAsync(Profile profile, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a Profile
        /// </summary>
        Task<bool> DeleteProfileAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

       
    }
}