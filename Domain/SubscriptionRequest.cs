using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class SubscriptionRequest
    {
        public string SubscriberId { get; set; }
        public string UserId { get; set; } // Assuming you store UserId in the database
    }
}
