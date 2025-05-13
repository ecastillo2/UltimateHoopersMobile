using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Follower
    {
        [Key]
        public string? FollowerId { get; set; }
        public string? UserId { get; set; }
        public string? FollowerUserId { get; set; }
        public string? FollowerDate { get; set; }
        public string? FollowerProfileId { get; set; }
        public string? ProfileId { get; set; }
    }
}
