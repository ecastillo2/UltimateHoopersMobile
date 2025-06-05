using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Subscription
    {
        [Key]
        public string? SubscriptionId { get; set; }
        public string? Name { get; set; }
        public double? Price { get; set; }
        public string? PaymentFrequency { get; set; }

    }
}
