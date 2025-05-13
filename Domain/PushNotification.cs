using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class PushNotification
    {
        [Key]
        public string? PushNotificationId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }

    }
}
