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
    public interface IClientApi
    {
        /// <summary>
        /// Get all Runs
        /// </summary>
        Task<List<Client>> GetClientsAsync(string accessToken, CancellationToken cancellationToken = default);

       
        Task<CursorPaginatedResultDto<ClientDetailViewModelDto>> GetClientsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Run by ID
        /// </summary>
        Task<Client> GetClientByIdAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new Run
        /// </summary>
        Task<Client> CreateClientAsync(Client run, string accessToken, CancellationToken cancellationToken = default);

      

        /// <summary>
        /// Update an existing Run
        /// </summary>
        Task<bool> UpdateClientAsync(Client run, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a Run
        /// </summary>
        Task<bool> DeleteClientAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}