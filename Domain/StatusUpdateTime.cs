using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class StatusUpdateTime
    {
        [Key]
        public string? StatusUpdateTimeId { get; set; }
        public DateTime? LastUpdateTime { get; set; }

    }
}
