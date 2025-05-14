
using Domain;

namespace DataLayer.DAL
{
    public interface IPushSubscriptionRepository : IDisposable
    {
        Task Subscribe(PushSubscription model);
        Task SendNotification(NotificationMessage model, string userId); 
        Task<int> Save();

    }
}
