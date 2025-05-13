using System;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    /// <summary>
    /// Represents a scouting report for a player profile
    /// </summary>
    public class ScoutingReport
    {
        /// <summary>
        /// Gets or sets the scouting report ID
        /// </summary>
        [Key]
        public string ScoutingReportId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the player strengths
        /// </summary>
        public string Strengths { get; set; }

        /// <summary>
        /// Gets or sets the player weaknesses
        /// </summary>
        public string Weaknesses { get; set; }

        /// <summary>
        /// Gets or sets additional notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the last modified date
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
    }
}