using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class RunViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public RunViewModelDto() { }

        // Existing constructor for mapping from Profile
        public RunViewModelDto(Run privateRun)
        {
            RunId = privateRun.RunId;
            CourtId = privateRun.CourtId;
            Name = privateRun.Name;
            ProfileId = privateRun.ProfileId;
            Status = privateRun.Status;
            RunDate = privateRun.RunDate;
            Cost = privateRun.Cost;
            Description = privateRun.Description;
            StartTime = privateRun.StartTime;
            EndTime = privateRun.EndTime;
            Type = privateRun.Type;
            CreatedDate = privateRun.CreatedDate;
            RunNumber = privateRun.RunNumber;
            SkillLevel = privateRun.SkillLevel;
            PaymentMethod = privateRun.PaymentMethod;
            TeamType = privateRun.TeamType;
            PlayerLimit = privateRun.PlayerLimit;
            Court = privateRun.Court;

        }

        public string? RunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }

        public decimal? Cost { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
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
        public Court Court { get; set; }
        public IList<JoinedRun> JoinedRunList { get; set; }
        [NotMapped]
        public string? ImageUrl { get; }
    
    }
}
