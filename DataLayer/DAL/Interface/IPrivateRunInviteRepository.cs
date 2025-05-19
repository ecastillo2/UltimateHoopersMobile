using Domain;

namespace DataLayer.DAL.Interface
{
    public interface IPrivateRunInviteRepository : IDisposable
    {
        Task<List<PrivateRunInvite>> GetPrivateRunInvites();
        Task<List<PrivateRunInvite>> GetPrivateRunInvitesByProfileId(string ProfileId);
        Task<PrivateRunInvite> GetPrivateRunInviteById(string PrivateRunInviteId);
        Task InsertPrivateRunInvite(PrivateRunInvite model);
        Task UpdatePlayerPrivateRunInvite(string ProfileId, string PrivateRunId, string AcceptedInvite);
        Task UpdatePlayerPresentPrivateRunInvite(string ProfileId, string PrivateRunId, bool Present);
        Task<bool> RemoveProfileFromPrivateRun(string ProfileId, string PrivateRunId);
        Task DeletePrivateRunInvite(string PrivateRunInviteId);
        Task ClearPrivateRunInviteByPrivateRun(string PrivateRunId);
        Task<bool> IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites(string profileId, string PrivateRunId);
        Task<int> Save();

    }
}
