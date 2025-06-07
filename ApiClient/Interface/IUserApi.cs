using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UltimateHoopers.Models;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Run API operations
    /// </summary>
    public interface IUserApi
    {
        
       
        Task<CursorPaginatedResultDto<UserDetailViewModelDto>> GetUsersWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Run by ID
        /// </summary>
        Task<User> GetUserByIdAsync(string userId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Users Search Async
        /// </summary>
        Task<List<User>> GetUsersSearchAsync(string searchQuery, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Run by ID
        /// </summary>
        Task<Profile> GetProfileByUserId(string userId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Run by ID
        /// </summary>
        Task<ScoutingReport> GetProfileScoutingReportByUserId(string userId, string accessToken, CancellationToken cancellationToken = default);


        /// <summary>
        /// Create a new Run
        /// </summary>
        Task<User> CreateUserAsync(User run, string accessToken, CancellationToken cancellationToken = default);

      

        /// <summary>
        /// Update an existing Run
        /// </summary>
        Task<bool> UpdateUserAsync(User run, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a Run
        /// </summary>
        Task<bool> DeleteUserAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}