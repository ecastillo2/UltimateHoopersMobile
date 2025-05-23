using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for JoinedRun repository operations
    /// </summary>
    public interface IJoinedRunRepository : IDisposable
    {
        /// <summary>
        /// Get all joined runs
        /// </summary>
        /// <returns>List of all joined runs</returns>
        Task<List<JoinedRun>> GetJoinedRuns();

        /// <summary>
        /// Get joined runs by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>List of joined runs for the specified profile</returns>
        Task<List<JoinedRun>> GetJoinedRunsByProfileId(string profileId);

        /// <summary>
        /// Get a joined run by ID
        /// </summary>
        /// <param name="joinedRunId">Joined run ID</param>
        /// <returns>Joined run with matching ID</returns>
        Task<JoinedRun> GetJoinedRunById(string joinedRunId);

        /// <summary>
        /// Insert a new joined run
        /// </summary>
        /// <param name="model">Joined run model to insert</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task InsertJoinedRun(CreateJoinedRunDto model);

        /// <summary>
        /// Update player joined run status
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="joinedRunId">Joined run ID</param>
        /// <param name="acceptedInvite">Accepted invite status</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdatePlayerJoinedRun(string profileId, string joinedRunId, string acceptedInvite);

        /// <summary>
        /// Update player present status in a joined run
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="joinedRunId">Joined run ID</param>
        /// <param name="present">Present status</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdatePlayerPresentJoinedRun(string profileId, string joinedRunId, bool present);

        /// <summary>
        /// Remove a profile from a run
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="runId">Run ID</param>
        /// <returns>True if profile was successfully removed, false otherwise</returns>
        Task<bool> RemoveProfileFromRun(string profileId, string runId);

        /// <summary>
        /// Delete a joined run
        /// </summary>
        /// <param name="joinedRunId">Joined run ID</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DeleteJoinedRun(string joinedRunId);

        /// <summary>
        /// Clear all joined runs for a specific run
        /// </summary>
        /// <param name="runId">Run ID</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ClearJoinedRunByRun(string runId);

        /// <summary>
        /// Check if profile is already invited to run
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="runId">Run ID</param>
        /// <returns>True if profile is already invited, false otherwise</returns>
        Task<bool> IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(string profileId, string runId);

        /// <summary>
        /// Get joined runs with player counts for a run
        /// </summary>
        /// <param name="runId">Run ID</param>
        /// <returns>List of profiles and status counts</returns>
        Task<(List<Profile> Profiles, int AcceptedCount, int UndecidedCount, int DeclinedCount)> GetJoinedRunsWithCountsByRunId(string runId);

        /// <summary>
        /// Save changes to the database
        /// </summary>
        /// <returns>Number of entities written to the database</returns>
        Task<int> Save();
    }
}