
using Domain;

namespace DataLayer.DAL
{
    public interface IHistoryRepository : IDisposable
    {
        Task<List<History>> GetHistorys();
        Task<History> GetHistoryById(string HistoryId);
        Task<List<History>> GetHistoryByProfileId(string ProfileyId);
        Task InsertHistory(History model);
        Task DeleteHistory(string HistoryId); 
        Task<int> Save();

    }
}
