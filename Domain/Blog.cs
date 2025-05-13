using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Blog
    {
        [Key]
        public string? BlogId { get; set; }
        public string? ImageURL { get; set; }
        public string? Title { get; set; }
        public string? PostText { get; set; }
        public string? Type { get; set; }
        public string? PostedDate { get; set; }
        public string? CreatedDate { get; set; }

    }
}
