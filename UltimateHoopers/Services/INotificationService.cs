using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltimateHoopers.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(string filter = "All");
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync();
        Task<int> GetUnreadCountAsync();
        Task<bool> UpdateNotificationSettingsAsync(NotificationSettings settings);
    }

    public class NotificationDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } = "general";
        public string Category { get; set; } = "All";
        public string EntityId { get; set; }
        public string IconText { get; set; } = "📩";
        public string IconBackgroundColor { get; set; } = "#2196F3"; // Default blue
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();

        // Computed property for UI
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;

                if (timeSpan.TotalMinutes < 1)
                    return "Just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} days ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
    }
}