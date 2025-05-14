using Domain;

namespace DataLayer.DAL
{
    public interface IActivityRepository : IDisposable
    {
        Task<List<Activity>> GetActivitys();
        Task<Activity> GetActivityById(string ActivityId);
        Task InsertActivity(Activity model);
        Task DeleteActivity(string NotificationId); 
        Task<int> Save();

    }
}
