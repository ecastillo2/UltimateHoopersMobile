using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Activity
    {
        [Key]
        public string? ActivityId { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
