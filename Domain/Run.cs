using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain
{
    public class Run
    {
        [Key]
        public string? RunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }
        
        public decimal? Cost { get; set; }
        public string? Name { get; set; }
       
        public string? Description { get; set; }
        public string? RunTime { get; set; }
        public string? EndTime { get; set; }
        public string? Type { get; set; }
        public string? CreatedDate { get; set; }
        public string? RunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public bool? IsPublic { get; set; }
        public int? PlayerLimit { get; set; }
        [NotMapped]
        public string? RelativeDate { get; set; }
        [NotMapped]
        public string? UserName { get; set; }

        [NotMapped]
        public Court? Court { get; set; }

        [NotMapped]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public string? InviteCount { get; set; }
        [NotMapped]
        public string? Password { get; set; }
        [NotMapped]
        public string? Token { get; set; }

        [NotMapped]
        public IList<JoinedRun>? JoinedRunList { get; set; }

        [NotMapped]
        public string? UserResponse { get; set; }

        [NotMapped]
        public int? AcceptedCount { get; set; }

        [NotMapped]
        public int? UndecidedCount { get; set; }
        [NotMapped]
        public int? DeclinedCount { get; set; }
        [NotMapped]
        public int? PlayerCount { get; set; }
       
    }
}
