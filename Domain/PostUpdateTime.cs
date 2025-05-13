using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class PostUpdateTime
    {
        [Key]
        public string? PostUpdateTimeId { get; set; }
        public DateTime? PostLastUpdateTime { get; set; }

    }
}
