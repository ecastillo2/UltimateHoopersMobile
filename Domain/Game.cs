using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Game
    {
        [Key]
        public string? GameId { get; set; }
        public string? CourtId { get; set; }
        public string? RunId { get; set; }
        public string? CreatedDate { get; set; }
        public string? WinProfileIdsStatusString { get; set; }
        public string? LoseProfileIdsStatusString { get; set; }
        public string? RunNumber { get; set; }
        public string? Location { get; set; }
        public string? GameNumber { get; set; }
        public string? Status { get; set; }

        [NotMapped]
        public string? UserWinOrLose { get; set; }
        [NotMapped]
        public List<Profile>? ProfileList { get; set; }
        [NotMapped]
        public List<Profile>? WinnersList { get; set; }
        [NotMapped]
        public List<Profile>? LossersList { get; set; }
        [NotMapped]
        public Run? Run { get; set; }
        [NotMapped]
        public Court? Court { get; set; }
    }
}