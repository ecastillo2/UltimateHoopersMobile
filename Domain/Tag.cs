using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Tag
    {
        [Key]
        public string? TagId { get; set; }
        public int? PostsWithTag { get; set; }
        public string? HashTag { get; set; }
        
    }
}
