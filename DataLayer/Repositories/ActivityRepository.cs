using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Activity entity operations
    /// </summary>
    public class ActivityRepository : GenericRepository<Activity>, IActivityRepository
    {
        public ActivityRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all activities
        /// </summary>
        public async Task<List<Activity>> GetActivitys()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Get activity by ID
        /// </summary>
        public async Task<Activity> GetActivityById(string activityId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.ActivityId == activityId);
        }

        /// <summary>
        /// Insert new activity
        /// </summary>
        public async Task InsertActivity(Activity activity)
        {
            if (string.IsNullOrEmpty(activity.ActivityId))
                activity.ActivityId = Guid.NewGuid().ToString();

            activity.CreatedDate = DateTime.Now;

            await base.AddAsync(activity);
            await SaveAsync();
        }

        /// <summary>
        /// Delete activity
        /// </summary>
        public async Task DeleteActivity(string activityId)
        {
            await DeleteByIdAsync(activityId);
            await SaveAsync();
        }
    }
}