using Domain;
using Domain.DtoModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltimateHoopers.Services
{
    public interface IJoinedRunService
    {
        /// <summary>
        /// Get all joined runs for a specific profile
        /// </summary>
        /// <param name="profileId">The profile ID</param>
        /// <returns>List of joined runs</returns>
        Task<List<JoinedRunDetailViewModelDto>> GetUserJoinedRunsAsync(string profileId);

        /// <summary>
        /// Remove a user from a joined run
        /// </summary>
        /// <param name="profileId">The profile ID</param>
        /// <param name="runId">The run ID</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveUserJoinRunAsync(string profileId, string runId);
    }
}