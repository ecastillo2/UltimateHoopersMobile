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
        /// Get Client By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Client> GetClientByIdAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Client Courts Async
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Court>> GetClientCourtsAsync(string clientId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> CreateClientAsync(Client client, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateClientAsync(Client client, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete Client Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> DeleteClientAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}