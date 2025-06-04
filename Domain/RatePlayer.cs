using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class RatePlayer
    {
        [Key]
        public string? RatePlayerId { get; set; }
        public string? ProfileId { get; set; }
        public string? RatedByProfileId { get; set; }
        public string? CreatedDate { get; set; }
        public string? StarRating { get; set; }
        public string? Comment { get; set; }
    }
}
