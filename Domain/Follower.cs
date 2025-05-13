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
        public string FollowerId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID being followed
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID of the follower
        /// </summary>
        public string FollowerProfileId { get; set; }

        /// <summary>
        /// Gets or sets the date when the follow relationship was created
        /// </summary>
        public string FollowDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the follow relationship
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the follow relationship
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}