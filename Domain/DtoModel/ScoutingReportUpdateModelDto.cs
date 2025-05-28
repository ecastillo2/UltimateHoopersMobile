using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ScoutingReportUpdateModelDto
    {
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

        public void UpdateScoutingReport(ScoutingReport report)
        {
            report.PrimaryPosition = PrimaryPosition;
            report.SecondaryPosition = SecondaryPosition;
            report.PlayingStyle = PlayingStyle;
            report.Shooting = Shooting;
            report.BallHandling = BallHandling;
            report.Passing = Passing;
            report.Defense = Defense;
            report.Redounding = Redounding;
            report.Athleticism = Athleticism;
            report.Strengths = Strengths;
            report.AreasforImprovement = AreasforImprovement;
            report.AdditionalNotes = AdditionalNotes;
            report.LastUpdated = LastUpdated;
        }
    }
}
