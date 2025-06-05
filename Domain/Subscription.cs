using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Subscription
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public Subscription() { }

        // Existing constructor for mapping from ScoutingReport
        public Subscription(Subscription subscription)
        {
            SubscriptionId = subscription.SubscriptionId;
            Name = subscription.Name;
            Price = subscription.Price;
            PaymentFrequency = subscription.PaymentFrequency;
            

        }

        [Key]
        public string? SubscriptionId { get; set; }
        public string? Name { get; set; }
        public double? Price { get; set; }
        public string? PaymentFrequency { get; set; }

    }
}
