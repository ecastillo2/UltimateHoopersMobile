using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class ScoutingReportRepository : IScoutingReportRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public ScoutingReportRepository(HUDBContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get ScoutingReport By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<ScoutingReport> GetScoutingReportById(string ScoutingReportId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.ScoutingReport
                                       where model.ScoutingReportId == ScoutingReportId
                                       select model).FirstOrDefaultAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }


      
        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateScoutingReport(ScoutingReport model)
        {
            using (var context = _context)
            {
                var existingItem = context.ScoutingReport.Where(s => s.ScoutingReportId == model.ScoutingReportId).FirstOrDefault<ScoutingReport>();

                if (existingItem != null)
                {
                    existingItem.PlayStyle = model.PlayStyle;
                    existingItem.StrengthOne = model.StrengthOne;
                    existingItem.StrengthTwo = model.StrengthTwo;
                    existingItem.WeaknessOne = model.WeaknessOne;
                    existingItem.WeaknessTwo = model.WeaknessTwo;
                    existingItem.PlayStyleImpactOne = model.PlayStyleImpactOne;
                    existingItem.PlayStyleImpactTwo = model.PlayStyleImpactTwo;
                    existingItem.Comparison = model.Comparison;
                    existingItem.Conclusion = model.Conclusion;
                    existingItem.Status = model.Status;
                    existingItem.IdealRole = model.IdealRole;
                    existingItem.LastUpdated = DateTime.Now;
                   

                    context.ScoutingReport.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Delete ScoutingReport
        /// </summary>
        /// <param name="CourtId"></param>
        /// <returns></returns>
        public async Task DeleteScoutingReport(string ScoutingReportId)
        {
            using (var context = _context)
            {
                ScoutingReport obj = (from u in context.ScoutingReport
                             where u.ScoutingReportId == ScoutingReportId
                                      select u).FirstOrDefault();

                _context.ScoutingReport.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
