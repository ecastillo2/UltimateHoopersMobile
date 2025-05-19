
using Domain;

namespace DataLayer.DAL.Interface
{
    public interface IGameRepository : IDisposable
    {
        Task<List<Game>> GetGames();
        Task<Game> GetGameById(string GameId);
        Task InsertGame(Game model);
        Task UpdateGame(Game model);
        Task DeleteGame(string GameId);
        Task<List<Game>> GetGameHistory();
        Task<List<Game>> GetGamesByProfileId(string ProfileId);
        Task<int> Save();

    }
}
