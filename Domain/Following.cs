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
        public string FollowingId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID of the follower
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID being followed
        /// </summary>
        public string FollowingProfileId { get; set; }

        /// <summary>
        /// Gets or sets the date when the following relationship was created
        /// </summary>
        public string FollowDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the following relationship
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the following relationship
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}