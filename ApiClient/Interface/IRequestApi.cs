using Azure;
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
    public interface IRequestApi
    {
        /// <summary>
        /// Get all Runs
        /// </summary>
        Task<List<Request>> GetRequestsAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Product DetailViewModelDto
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CursorPaginatedResultDto<RequestDetailViewModelDto>> GetRequestsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            string accessToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Request By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Request> GetRequestByIdAsync(string runId, string accessToken, CancellationToken cancellationToken = default);

      
        /// <summary>
        /// Create Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> CreateRequestAsync(Request product, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="run"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateRequestAsync(Request product, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete Request Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(bool Success, string ErrorMessage)> DeleteRequestAsync(string productId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}