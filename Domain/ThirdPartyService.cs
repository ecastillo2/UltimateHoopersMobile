using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class ThirdPartyService
    {
        [Key]
        public string? ThirdPartyServiceId { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public decimal? Cost { get; set; }
        public string? PaymentFrequency { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }

        public string? Type { get; set; }

    }
}
