using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class PrivateRun
    {
        [Key]
        public string? PrivateRunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }
        
        public decimal? Cost { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? RunTime { get; set; }
        public string? EndTime { get; set; }
        public string? Type { get; set; }
        public string? CreatedDate { get; set; }
        public string? PrivateRunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public int? PlayerLimit { get; set; }
        [NotMapped]
        public string? RelativeDate { get; set; }
        [NotMapped]
        public string? UserName { get; set; }

        [NotMapped]
        public Court? Court { get; set; }

        [NotMapped]
        public string? ImageURL { get; set; }

        [NotMapped]
        public string? InviteCount { get; set; }
        [NotMapped]
        public string? Password { get; set; }
        [NotMapped]
        public string? Token { get; set; }

        [NotMapped]
        public IList<PrivateRunInvite>? PrivateRunInviteList { get; set; }

        [NotMapped]
        public string? UserResponse { get; set; }

        [NotMapped]
        public int? AcceptedCount { get; set; }

        [NotMapped]
        public int? UndecidedCount { get; set; }
        [NotMapped]
        public int? DeclinedCount { get; set; }
    }
}
