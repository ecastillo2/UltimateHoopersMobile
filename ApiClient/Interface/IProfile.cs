using Domain;
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