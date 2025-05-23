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
    public interface IJoinedRunApi
    {


        /// <summary>
        /// Get Run by ID
        /// </summary>
        Task<List<JoinedRun>> GetUserJoinedRunsAsync(string profileId, string accessToken, CancellationToken cancellationToken = default);

 

        /// <summary>
        /// Delete a Run
        /// </summary>
        Task<bool> RemoveUserJoinRunAsync(string profileId, string runId, string accessToken, CancellationToken cancellationToken = default);

        
    }
}