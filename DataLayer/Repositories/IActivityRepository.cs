using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for Activity repository
    /// </summary>
    public interface IActivityRepository : IGenericRepository<Activity>
    {
        Task<List<Activity>> GetActivitys();
        Task<Activity> GetActivityById(string activityId);
        Task InsertActivity(Activity activity);
        Task DeleteActivity(string activityId);
    }
}