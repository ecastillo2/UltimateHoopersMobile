using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class PrivateRunViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public PrivateRunViewModelDto() { }

        // Existing constructor for mapping from Profile
        public PrivateRunViewModelDto(PrivateRun privateRun)
        {
            PrivateRunId = privateRun.PrivateRunId;
            CourtId = privateRun.CourtId;
            ProfileId = privateRun.ProfileId;
            Status = privateRun.Status;
            RunDate = privateRun.RunDate;
            Cost = privateRun.Cost;

            Description = privateRun.Description;
            RunTime = privateRun.RunTime;
            EndTime = privateRun.EndTime;
            Type = privateRun.Type;
            CreatedDate = privateRun.CreatedDate;
            PrivateRunNumber = privateRun.PrivateRunNumber;
            SkillLevel = privateRun.SkillLevel;
            PaymentMethod = privateRun.PaymentMethod;
            TeamType = privateRun.TeamType;
            PlayerLimit = privateRun.PlayerLimit;
          

        }

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
    }
}
