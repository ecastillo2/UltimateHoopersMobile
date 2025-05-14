using Domain;

namespace DataLayer.DAL
{
    public interface ICourtRepository : IDisposable
    {
        Task<List<Court>> GetCourts();
        Task<Court> GetCourtById(string CourtId);
        Task InsertCourt(Court model);
        Task DeleteCourt(string CourtId);
        Task UpdateCourt(Court model);
        Task<int> Save();

    }
}
