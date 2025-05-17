using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class PrivateRunUpdateModelDto
    {
        public string? PrivateRunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }

        public decimal? Cost { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? RunTime { get; set; }
        public string? EndTime { get; set; }
        public string? Type { get; set; }
        public string? CreatedDate { get; set; }
        public string? PrivateRunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public int? PlayerLimit { get; set; }

        public void UpdatePrivateRun(PrivateRun privateRun)
        {
            privateRun.Status = Status;
            privateRun.Cost = Cost;
            privateRun.Status = Status;
            privateRun.Title = Title;
            privateRun.Location = Location;
            privateRun.Description = Description;
            privateRun.RunTime = RunTime;
            privateRun.EndTime = EndTime;
            privateRun.Type = Type;
            privateRun.SkillLevel = SkillLevel;
            privateRun.PaymentMethod = PaymentMethod;
            privateRun.PlayerLimit = PlayerLimit;
        }
    }
}
