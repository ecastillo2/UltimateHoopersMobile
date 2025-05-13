using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Notification
    {
        [Key]
        public string? NotificationId { get; set; }
        public string? PrivateRunId { get; set; }
        public string? ProfileId { get; set; }
        public string? Title { get; set; }
        public string? ImageURL { get; set; }
        public string? Description { get; set; }
        public string? CreatedDate { get; set; }
        public string? Type { get; set; }
        public string? RatePlayerProfileId { get; set; }
        public string? Status { get; set; }
        public bool? Read { get; set; }
        public string? OrderId { get; set; }
        public string? Category { get; set; }

        [NotMapped]
        public string? RelativeTime { get; set; }
        [NotMapped]
        public string? UserName { get; set; }
        [NotMapped]
        public Profile? Profile { get; set; }

        [NotMapped]
        public Profile? RateProfile { get; set; }
    }
}
