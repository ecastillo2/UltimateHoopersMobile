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
        public RunViewModelDto(Run run)
        {
            RunId = run.RunId;
            CourtId = run.CourtId;
            ClientId = run.ClientId;
            Name = run.Name;
            ProfileId = run.ProfileId;
            Status = run.Status;
            RunDate = run.RunDate;
            Cost = run.Cost;
            Description = run.Description;
            StartTime = run.StartTime;
            EndTime = run.EndTime;
            Type = run.Type;
            CreatedDate = run.CreatedDate;
            RunNumber = run.RunNumber;
            SkillLevel = run.SkillLevel;
            PaymentMethod = run.PaymentMethod;
            TeamType = run.TeamType;
            PlayerLimit = run.PlayerLimit;
            Court = run.Court;
            Occurrence = run.Occurrence;

        }

        public string? RunId { get; set; }
        public string? CourtId { get; set; }
        public string? ClientId { get; set; }
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
        public DateTime? CreatedDate { get; set; }
        public string? RunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public int? PlayerLimit { get; set; }
        public string? Occurrence { get; set; }
        public bool? IsPublic { get; set; }
        public Court Court { get; set; }
        public Client Client { get; set; }
        public IList<JoinedRun> JoinedRunList { get; set; }
        [NotMapped]
        public string? ImageUrl { get; }
    
    }
}
