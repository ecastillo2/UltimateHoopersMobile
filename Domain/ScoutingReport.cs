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
            PrimaryPosition = report.PrimaryPosition;
            SecondaryPosition = report.SecondaryPosition;
            PlayingStyle = report.PlayingStyle;
            Shooting = report.Shooting;
            BallHandling = report.BallHandling;
            Passing = report.Passing;
            Defense = report.Defense;
            Redounding = report.Redounding;
            Athleticism = report.Athleticism;
            Strengths = report.Strengths;
            AreasforImprovement = report.AreasforImprovement;
            AdditionalNotes = report.AdditionalNotes;
            LastUpdated = report.LastUpdated;

        }


        [Key]
        public string? ScoutingReportId { get; set; }
        public string? ProfileId { get; set; }
        public string? PrimaryPosition { get; set; }
        public string? SecondaryPosition { get; set; }
        public string? PlayingStyle { get; set; }
        public int? Shooting { get; set; }
        public int? BallHandling { get; set; }
        public int? Passing { get; set; }
        public int? Defense { get; set; }
        public int? Redounding { get; set; }
        public int? Athleticism { get; set; }
        public string? Strengths { get; set; }
        public string? AreasforImprovement { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime? LastUpdated { get; set; }

        [NotMapped]
        public Profile? Profile { get; set; }

        [NotMapped]
        public string? SubId { get; set; }
    }
}