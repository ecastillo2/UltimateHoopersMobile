using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    /// <summary>
    /// Represents a user profile in the system
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Gets or sets the profile ID
        /// </summary>
        [Key]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with this profile
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the full name (not stored in database)
        /// </summary>
        [NotMapped]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// Gets or sets the weight
        /// </summary>
        public string Weight { get; set; }

        /// <summary>
        /// Gets or sets the position
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Gets or sets the ranking
        /// </summary>
        public string Ranking { get; set; }

        /// <summary>
        /// Gets or sets the star rating
        /// </summary>
        public string StarRating { get; set; }

        /// <summary>
        /// Gets or sets the QR code
        /// </summary>
        public string QRCode { get; set; }

        /// <summary>
        /// Gets or sets the bio
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Gets or sets the image URL
        /// </summary>
        public string ImageURL { get; set; }

        /// <summary>
        /// Gets or sets the player archetype
        /// </summary>
        public string PlayerArchetype { get; set; }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the zip code
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the player number
        /// </summary>
        public string PlayerNumber { get; set; }

        /// <summary>
        /// Gets or sets the points
        /// </summary>
        public int? Points { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this profile has the top record
        /// </summary>
        public bool? TopRecord { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the followers count (not stored in database)
        /// </summary>
        [NotMapped]
        public string FollowersCount { get; set; }

        /// <summary>
        /// Gets or sets the following count (not stored in database)
        /// </summary>
        [NotMapped]
        public string FollowingCount { get; set; }

        /// <summary>
        /// Gets or sets the total games (not stored in database)
        /// </summary>
        [NotMapped]
        public string TotalGames { get; set; }

        /// <summary>
        /// Gets or sets the total wins (not stored in database)
        /// </summary>
        [NotMapped]
        public int TotalWins { get; set; }

        /// <summary>
        /// Gets or sets the total losses (not stored in database)
        /// </summary>
        [NotMapped]
        public int TotalLosses { get; set; }

        /// <summary>
        /// Gets or sets the win percentage (not stored in database)
        /// </summary>
        [NotMapped]
        public string WinPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this profile is followed by the current user (not stored in database)
        /// </summary>
        [NotMapped]
        public bool Followed { get; set; }

        /// <summary>
        /// Gets or sets the invite status (not stored in database)
        /// </summary>
        [NotMapped]
        public string InviteStatus { get; set; }

        /// <summary>
        /// Gets or sets the setting
        /// </summary>
        public Setting Setting { get; set; }

        /// <summary>
        /// Gets or sets the scouting report
        /// </summary>
        public ScoutingReport ScoutingReport { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the last modified date
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
    }
}