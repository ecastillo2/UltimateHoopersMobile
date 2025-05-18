using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain
{
    /// <summary>
    /// Represents user profile settings
    /// </summary>
    public class Setting
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public Setting() { }

        // Existing constructor for mapping from ScoutingReport
        public Setting(Setting report)
        {
            SettingId = report.SettingId;
            ProfileId = report.ProfileId;
            AllowComments = report.AllowComments;
            ShowGameHistory = report.ShowGameHistory;
            ShowGameHistory = report.ShowGameHistory;
            AllowEmailNotification = report.AllowEmailNotification;
           
        }

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