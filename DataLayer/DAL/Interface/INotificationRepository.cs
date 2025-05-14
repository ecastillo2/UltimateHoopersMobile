
using Domain;

namespace DataLayer.DAL
{
    public interface INotificationRepository : IDisposable
    {
        Task<List<Notification>> GetNotifications();
        Task InsertNotification(Notification model);
        Task<List<Notification>>  GetNotificationByProfileId(string ProfileId);
        Task<Notification> GetNotificationById(string NotificationId);
        Task DeleteNotification(string NotificationId);
        Task UpdateNotification(Notification model);
        Task UpdateNotificationRead(string NotificationId);
        Task<int> Save();

    }
}
