using Domain;

namespace DataLayer.DAL
{
    public interface IPrivateRunRepository : IDisposable
    {
        Task<List<PrivateRun>> GetPrivateRuns();
        Task<List<PrivateRun>> GetPrivateRunsByProfileId(string ProfileId);
        Task<List<PrivateRun>> GetProfileInvitesByProfileId(string ProfileId);
        Task<List<Profile>> GetProfilesByPrivateRunId(string privateRunId);
        Task<PrivateRun> GetPrivateRunById(string PrivateRunId);
        Task InsertPrivateRun(PrivateRun model);
        Task RemovePrivateRun(string PrivateRunId);
        Task UpdatePrivateRun(PrivateRun model);
        Task<int> Save();

    }
}
