using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Rating
    {
        [Key]
        public string? RatingId { get; set; }
        public string? ProfileId { get; set; }
        public string? RatedByProfileId { get; set; }
        public string? CreatedDate { get; set; }
        public string? StarRating { get; set; }
    }
}
