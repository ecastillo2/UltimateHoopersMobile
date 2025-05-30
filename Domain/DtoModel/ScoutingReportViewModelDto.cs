using System;
using System.Text.Json.Serialization;

namespace Domain.DtoModel
{
    public class ScoutingReportViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public ScoutingReportViewModelDto() { }

        // Existing constructor for mapping from ScoutingReport
        public ScoutingReportViewModelDto(ScoutingReport report)
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
            Rebounding = report.Rebounding;
            Athleticism = report.Athleticism;
            Strengths = report.Strengths;
            AreasforImprovement = report.AreasforImprovement;
            AdditionalNotes = report.AdditionalNotes;
            LastUpdated = report.LastUpdated;
          
        }

        public string? ScoutingReportId { get; set; }
        public string? ProfileId { get; set; }
        public string? PrimaryPosition { get; set; }
        public string? SecondaryPosition { get; set; }
        public string? PlayingStyle { get; set; }
        public int? Shooting { get; set; }
        public int? BallHandling { get; set; }
        public int? Passing { get; set; }
        public int? Defense { get; set; }
        public int? Rebounding { get; set; }
        public int? Athleticism { get; set; }
        public string? Strengths { get; set; }
        public string? AreasforImprovement { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime? LastUpdated { get; set; }
        public ScoutingReport ScoutingReport { get; set; }
    }
}