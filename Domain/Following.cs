using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Following
    {
        [Key]
        public string? FollowingId { get; set; }
        public string? UserId { get; set; }
        public string? ProfileId { get; set; }
        public string? FollowingUserId { get; set; }
        public string? FollowingProfileId { get; set; }
        public string? FollowingDate { get; set; }
    }
}
