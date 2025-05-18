using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    /// <summary>
    /// Represents a scouting report for a player profile
    /// </summary>
    public class ScoutingReport
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public ScoutingReport() { }

        // Existing constructor for mapping from ScoutingReport
        public ScoutingReport(ScoutingReport report)
        {
            ScoutingReportId = report.ScoutingReportId;
            ProfileId = report.ProfileId;
            PlayStyle = report.PlayStyle;
            StrengthOne = report.StrengthOne;
            StrengthTwo = report.StrengthTwo;
            WeaknessOne = report.WeaknessOne;

        }


        [Key]
        public string? ScoutingReportId { get; set; }
        public string? ProfileId { get; set; }
        public string? PlayStyle { get; set; }
        public string? StrengthOne { get; set; }
        public string? StrengthTwo { get; set; }
        public string? WeaknessOne { get; set; }
        public string? WeaknessTwo { get; set; }
        public string? PlayStyleImpactOne { get; set; }
        public string? PlayStyleImpactTwo { get; set; }
        public string? Comparison { get; set; }
        public string? Conclusion { get; set; }
        public string? Status { get; set; }

        public string? IdealRole { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        [NotMapped]
        public Profile? Profile { get; set; }

        [NotMapped]
        public string? SubId { get; set; }
    }
}