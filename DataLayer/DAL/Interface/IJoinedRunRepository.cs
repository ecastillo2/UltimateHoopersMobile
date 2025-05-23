using Domain;

namespace DataLayer.DAL.Interface
{
    public interface IJoinedRunRepository : IDisposable
    {
        Task<List<JoinedRun>> GetJoinedRuns();
        Task<List<JoinedRun>> GetJoinedRunsByProfileId(string ProfileId);
        Task<JoinedRun> GetJoinedRunById(string JoinedRunId);
        Task InsertJoinedRun(JoinedRun model);
        Task UpdatePlayerJoinedRun(string ProfileId, string RunId, string AcceptedInvite);
        Task UpdatePlayerPresentJoinedRun(string ProfileId, string RunId, bool Present);
        Task<bool> RemoveProfileFromRun(string ProfileId, string RunId);
        Task DeleteJoinedRun(string JoinedRunId);
        Task ClearJoinedRunByRun(string RunId);
        Task<bool> IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(string profileId, string RunId);
        Task<int> Save();

    }
}
