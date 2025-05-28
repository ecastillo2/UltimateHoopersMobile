using System;

namespace Domain
{
    /// <summary>
    /// Represents a following relationship between profiles
    /// </summary>
    public class Following
    {

        /// <summary>
        /// Gets or sets the following ID
        /// </summary>
        public string? FollowingId { get; set; }

        /// <summary>
        /// Gets or sets the following ID
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID of the follower
        /// </summary>
        public string? ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID being followed
        /// </summary>
        public string? FollowingProfileId { get; set; }

      
        public string? FollowingUserId { get; set; }

    
    }
}