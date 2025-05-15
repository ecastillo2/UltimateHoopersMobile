using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    /// <summary>
    /// Represents user profile settings
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Gets or sets the setting ID
        /// </summary>
        [Key]
        public string SettingId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow comments on posts
        /// </summary>
        public bool AllowComments { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show game history
        /// </summary>
        public bool ShowGameHistory { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to allow email notifications
        /// </summary>
        public bool AllowEmailNotification { get; set; } = true;

        
    }
}