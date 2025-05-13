using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Cart
    {
        [Key]
        public string? CartId { get; set; }
        public string? OrderId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
