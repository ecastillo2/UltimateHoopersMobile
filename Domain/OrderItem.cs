using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class OrderItem
    {
        [Key]
        public string? OrderItemId { get; set; }
        public string? OrderId { get; set; }
        public string? ProductId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
       

    }
}
