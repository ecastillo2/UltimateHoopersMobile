using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Activity repository
    /// </summary>
    public interface IActivityRepository : IGenericRepository<Activity>
    {
        /// <summary>
        /// Get all activities
        /// </summary>
        Task<List<Activity>> GetActivitys();

        /// <summary>
        /// Get activity by ID
        /// </summary>
        Task<Activity> GetActivityById(string activityId);

        /// <summary>
        /// Insert new activity
        /// </summary>
        Task InsertActivity(Activity activity);

        /// <summary>
        /// Delete activity
        /// </summary>
        Task DeleteActivity(string activityId);
    }
}