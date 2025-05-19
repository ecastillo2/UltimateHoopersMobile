using Domain;

namespace UltimateHoopers.Services
{
    public interface IPrivateRunService
    {
        Task<List<PrivateRun>> GetPrivateRunsAsync();
        Task<PrivateRun> GetPrivateRunByIdAsync(string PrivateRunId);
        Task<PrivateRun> CreatePrivateRunAsync(PrivateRun PrivateRun);
        Task<bool> UpdatePrivateRunAsync(PrivateRun PrivateRun);
        Task<bool> DeletePrivateRunAsync(string PrivateRunId);
    }
}