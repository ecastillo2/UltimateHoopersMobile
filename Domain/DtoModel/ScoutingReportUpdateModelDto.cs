using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ScoutingReportUpdateModelDto
    {
        public string PlayStyle { get; set; }
        public string StrengthOne { get; set; }
        public string StrengthTwo { get; set; }
        public string WeaknessOne { get; set; }
        public string WeaknessTwo { get; set; }
        public string PlayStyleImpactOne { get; set; }
        public string PlayStyleImpactTwo { get; set; }
        public string Comparison { get; set; }
        public string Conclusion { get; set; }
        public string IdealRole { get; set; }

        public void UpdateScoutingReport(ScoutingReport report)
        {
            report.PlayStyle = PlayStyle;
            report.StrengthOne = StrengthOne;
            report.StrengthTwo = StrengthTwo;
            report.WeaknessOne = WeaknessOne;
            report.WeaknessTwo = WeaknessTwo;
            report.PlayStyleImpactOne = PlayStyleImpactOne;
            report.PlayStyleImpactTwo = PlayStyleImpactTwo;
            report.Comparison = Comparison;
            report.Conclusion = Conclusion;
            report.IdealRole = IdealRole;
        }
    }
}
