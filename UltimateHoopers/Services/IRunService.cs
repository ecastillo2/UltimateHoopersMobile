using Domain;
using UltimateHoopers.Models;

namespace UltimateHoopers.Services
{
    public interface IRunService
    {
        Task<List<Run>> GetRunsAsync();
        Task<Run> GetRunByIdAsync(string RunId);
        Task<Run> CreateRunAsync(Run PrivateRun);
        Task<bool> UpdateRunAsync(Run PrivateRun);
        Task<bool> DeleteRunAsync(string RunId);
        Task<bool> UserJoinRunAsync(JoinedRun PrivateRun);
    }
}