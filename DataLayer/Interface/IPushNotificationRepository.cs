using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Interface for PushNotification repository
    /// </summary>
    public interface IPushNotificationRepository : IGenericRepository<PushNotification>
    {
        // Add any push notification-specific repository methods here
    }
}