using Domain;
using Domain.DtoModel;
using UltimateHoopers.Models;

namespace UltimateHoopers.Services
{
    public interface IJoinedRunService
    {

        Task<List<JoinedRun>> GetUserJoinedRunsAsync(string ProfileId);
        Task<bool> RemoveUserJoinRunAsync(string ProfileId,string RunId);
        
    }
}