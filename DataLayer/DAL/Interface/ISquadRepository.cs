using Domain;

namespace DataLayer.DAL.Interface
{
    public interface ISquadRepository : IDisposable
    {
        Task<List<Squad>> GetSquads();
        Task<List<SquadTeam>> GetSquadTeams();
        Task<Squad> GetSquadById(string SquadId);
        Task<List<SquadTeam>> GetPendingRequestsProfileById(string ProfileId);
        Task<Squad> GetSquadByOwnerProfileId(string profileId);
        Task<string> AddPlayerToSquad(string ProfileId, string SquadId);
        Task SendPlayerRequestToJoinSquad(string ProfileId, string SquadId);
        Task ClearSquad(string SquadId);
        Task<string> RemovePlayerFromSquad(string profileId, string SquadId);
        Task<int> Save();

    }
}
