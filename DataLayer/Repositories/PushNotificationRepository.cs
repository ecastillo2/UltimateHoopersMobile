using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for PushNotification entity operations
    /// </summary>
    public class PushNotificationRepository : GenericRepository<PushNotification>, IPushNotificationRepository
    {
        public PushNotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get push notification by ID
        /// </summary>
        public override async Task<PushNotification> GetByIdAsync(object id)
        {
            string pushNotificationId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(pn => pn.PushNotificationId == pushNotificationId);
        }

        /// <summary>
        /// Add new push notification
        /// </summary>
        public override async Task AddAsync(PushNotification pushNotification)
        {
            if (string.IsNullOrEmpty(pushNotification.PushNotificationId))
                pushNotification.PushNotificationId = Guid.NewGuid().ToString();

            await base.AddAsync(pushNotification);
        }
    }
}