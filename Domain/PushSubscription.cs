using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class PushSubscription
    {
        [Key]
        public int? SubscriptionId { get; set; }
        public string? UserId { get; set; }
        public string? Endpoint { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string? P256dh { get; set; }
        public string? Auth { get; set; }

    }
}
