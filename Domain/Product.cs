using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Product
    {
        [Key]
        public string? ProductId { get; set; }
        public string? ImageURL { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? ProductNumber { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public int? Points { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? ImageUrlName { get; set; }
    }
}
