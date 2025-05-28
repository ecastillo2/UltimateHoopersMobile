using System;

namespace Domain
{
    /// <summary>
    /// Represents a follower relationship between profiles
    /// </summary>
    public class Follower
    {
        /// <summary>
        /// Gets or sets the follower ID
        /// </summary>
        public string? FollowerId { get; set; }

        public string? FollowerUserId { get; set; }

        public string? UserId { get; set; }
        /// <summary>
        /// Gets or sets the profile ID being followed
        /// </summary>
        public string? ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID of the follower
        /// </summary>
        public string? FollowerProfileId { get; set; }


     
    }
}