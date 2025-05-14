using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain;

namespace DataLayer.DAL
{
    public class ActivityRepository : IActivityRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       

        public ActivityRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        public async Task<List<Activity>> GetActivitys()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.Activity.ToListAsync();

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
        /// Insert Contact
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertActivity(Activity model)
        {
            using (var context = _context)
            {
                try
                {
                    model.ActivityId = Guid.NewGuid().ToString();
                    model.CreatedDate = DateTime.Now;

                    await context.Activity.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }



        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Activity> GetActivityById(string ActivityId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Activity
                                       where model.ActivityId == ActivityId
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
        /// Delete Contact
        /// </summary>
        /// <param name="ContactId"></param>
        /// <returns></returns>
        public async Task DeleteActivity(string ActivityId)
        {
            using (var context = _context)
            {
                Activity obj = (from u in context.Activity
                                where u.ActivityId == ActivityId
                                select u).FirstOrDefault();



                _context.Activity.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
         
    }
}
