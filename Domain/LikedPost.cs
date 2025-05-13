using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class LikedPost
    {
        [Key]
        public string? LikedPostId { get; set; }
        public string? PostId { get; set; }
        public string? LikedByUserId { get; set; }
        public string? LikedDate { get; set; }
        public string? LikedByProfileId { get; set; }
    }
}
