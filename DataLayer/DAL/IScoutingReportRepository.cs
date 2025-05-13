using Domain;

namespace DataLayer.DAL
{
    public interface IScoutingReportRepository : IDisposable
    {
        Task<ScoutingReport> GetScoutingReportById(string ScoutingReportId);
        Task DeleteScoutingReport(string ScoutingReportId);
        Task UpdateScoutingReport(ScoutingReport model);
        Task<int> Save();

    }
}
