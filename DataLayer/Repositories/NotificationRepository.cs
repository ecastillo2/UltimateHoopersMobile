using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Common;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Notification entity operations
    /// </summary>
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all notifications with profile information
        /// </summary>
        public override async Task<List<Notification>> GetAllAsync()
        {
            var notifications = await _dbSet.ToListAsync();

            // Get profile IDs
            var profileIds = notifications
                .Where(n => !string.IsNullOrEmpty(n.ProfileId))
                .Select(n => n.ProfileId)
                .Distinct()
                .ToList();

            // Get profiles in one query
            var profiles = await _context.Profiles
                .Where(p => profileIds.Contains(p.ProfileId))
                .ToDictionaryAsync(p => p.ProfileId);

            // Add profile info to notifications
            foreach (var notification in notifications)
            {
                if (!string.IsNullOrEmpty(notification.ProfileId) &&
                    profiles.TryGetValue(notification.ProfileId, out var profile))
                {
                    notification.Profile = profile;
                }
            }

            return notifications;
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        public override async Task<Notification> GetByIdAsync(object id)
        {
            string notificationId = id.ToString();
            var notification = await _dbSet
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
                return null;

            // Calculate relative time
            if (DateTime.TryParse(notification.CreatedDate, out var createdDate))
            {
                notification.RelativeTime = RelativeTime.GetRelativeTime(createdDate, "America/New_York");
            }
            else
            {
                notification.RelativeTime = "Unknown";
            }

            return notification;
        }

        /// <summary>
        /// Get notifications by profile ID
        /// </summary>
        public async Task<List<Notification>> GetByProfileIdAsync(string profileId)
        {
            var notifications = await _dbSet
                .Where(n => n.ProfileId == profileId)
                .ToListAsync();

            // Calculate relative time for each notification
            foreach (var notification in notifications)
            {
                if (DateTime.TryParse(notification.CreatedDate, out var createdDate))
                {
                    notification.RelativeTime = RelativeTime.GetRelativeTime(createdDate, "America/New_York");
                }
                else
                {
                    notification.RelativeTime = "Unknown";
                }
            }

            return notifications;
        }

        /// <summary>
        /// Add new notification
        /// </summary>
        public override async Task AddAsync(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.NotificationId))
                notification.NotificationId = Guid.NewGuid().ToString();

            notification.CreatedDate = DateTime.Now.ToString();

            await base.AddAsync(notification);
        }

        /// <summary>
        /// Update notification
        /// </summary>
        public async Task UpdateNotificationAsync(Notification notification)
        {
            var existingNotification = await GetByIdAsync(notification.NotificationId);
            if (existingNotification == null)
                return;

            // Update properties
            existingNotification.Read = notification.Read;
            existingNotification.Title = notification.Title;
            existingNotification.Description = notification.Description;
            existingNotification.Status = notification.Status;

            _dbSet.Update(existingNotification);
            await SaveAsync();
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task MarkAsReadAsync(string notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null)
                return;

            notification.Read = true;

            _dbSet.Update(notification);
            await SaveAsync();
        }
    }

    /// <summary>
    /// Interface for Notification repository
    /// </summary>
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetByProfileIdAsync(string profileId);
        Task UpdateNotificationAsync(Notification notification);
        Task MarkAsReadAsync(string notificationId);
    }
}