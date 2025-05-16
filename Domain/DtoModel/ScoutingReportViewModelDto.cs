using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ScoutingReportViewModelDto
    {
        public string ScoutingReportId { get; set; }
        public string ProfileId { get; set; }
        public string PlayStyle { get; set; }
        public string StrengthOne { get; set; }
        public string StrengthTwo { get; set; }
        public string WeaknessOne { get; set; }
        public string WeaknessTwo { get; set; }
        public string PlayStyleImpactOne { get; set; }
        public string PlayStyleImpactTwo { get; set; }
        public string Comparison { get; set; }
        public string Conclusion { get; set; }
        public string Status { get; set; }
        public string IdealRole { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        public ScoutingReportViewModelDto(ScoutingReport report)
        {
            ScoutingReportId = report.ScoutingReportId;
            ProfileId = report.ProfileId;
            PlayStyle = report.PlayStyle;
            StrengthOne = report.StrengthOne;
            StrengthTwo = report.StrengthTwo;
            WeaknessOne = report.WeaknessOne;
            WeaknessTwo = report.WeaknessTwo;
            PlayStyleImpactOne = report.PlayStyleImpactOne;
            PlayStyleImpactTwo = report.PlayStyleImpactTwo;
            Comparison = report.Comparison;
            Conclusion = report.Conclusion;
            Status = report.Status;
            IdealRole = report.IdealRole;
            CreatedDate = report.CreatedDate;
            LastUpdated = report.LastUpdated;
        }
    }
}
