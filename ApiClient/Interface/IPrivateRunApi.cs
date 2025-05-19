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
    /// Interface for PrivateRun API operations
    /// </summary>
    public interface IPrivateRunApi
    {
        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        Task<List<PrivateRun>> GetPrivateRunsAsync(string accessToken, CancellationToken cancellationToken = default);

       
        Task<CursorPaginatedResultDto<PrivateRunViewModelDto>> GetPrivateRunsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<PrivateRun> GetPrivateRunByIdAsync(string privateRunId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new PrivateRun
        /// </summary>
        Task<PrivateRun> CreatePrivateRunAsync(PrivateRun privateRun, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing PrivateRun
        /// </summary>
        Task<bool> UpdatePrivateRunAsync(PrivateRun privateRun, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a PrivateRun
        /// </summary>
        Task<bool> DeletePrivateRunAsync(string privateRunId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}