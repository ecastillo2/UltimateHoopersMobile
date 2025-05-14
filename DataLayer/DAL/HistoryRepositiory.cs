using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class HistoryRepository : IHistoryRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       

        public HistoryRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get History By Id
        /// </summary>
        /// <param name="HistoryId"></param>
        /// <returns></returns>
        public async Task<History> GetHistoryById(string HistoryId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.History
                                       where model.HistoryId == HistoryId
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
        /// Get Historys
        /// </summary>
        /// <returns></returns>
        public async Task<List<History>> GetHistorys()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.History
                                       select model).ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Insert History
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertHistory(History model)
        {
            using (var context = _context)
            {
                try
                {
                    model.HistoryId = Guid.NewGuid().ToString();

                    await context.History.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Get History By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<List<History>> GetHistoryByProfileId(string profileId)
        {
            // Ensure that the ProfileId is not null or empty
            if (string.IsNullOrEmpty(profileId))
            {
                return new List<History>();
            }

            // Assuming the DbContext is named 'ApplicationDbContext' with a DbSet<History> property
            using (var context = _context)
            {
                // Query the History table to find entries that match the given ProfileId
                var historyList = await context.History
                    .Where(h => h.ProfileId == profileId)
                    .OrderByDescending(h => h.CreatedDate) // Order by CreatedDate or another relevant field
                    .ToListAsync();

                return historyList;
            }
        }

        /// <summary>
        /// Delete History
        /// </summary>
        /// <param name="HistoryId"></param>
        /// <returns></returns>
        public async Task DeleteHistory(string HistoryId)
        {
            using (var context = _context)
            {
                History obj = (from u in context.History
                               where u.HistoryId == HistoryId
                               select u).FirstOrDefault();



                _context.History.Remove(obj);
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
