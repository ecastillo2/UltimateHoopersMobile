using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class RunUpdateModelDto
    {
        public string? RunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }

        public decimal? Cost { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Type { get; set; }
        public string? CreatedDate { get; set; }
        public string? RunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public int? PlayerLimit { get; set; }

        public void UpdateRun(Run run)
        {
            run.Status = Status;
            run.Cost = Cost;
            run.Status = Status;

            run.Description = Description;
            run.StartTime = StartTime;
            run.EndTime = EndTime;
            run.Type = Type;
            run.SkillLevel = SkillLevel;
            run.PaymentMethod = PaymentMethod;
            run.PlayerLimit = PlayerLimit;
        }
    }
}
