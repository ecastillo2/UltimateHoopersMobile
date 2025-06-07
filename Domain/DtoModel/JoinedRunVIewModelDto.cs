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
    public class JoinedRunViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public JoinedRunViewModelDto() { }

        // Existing constructor for mapping from Profile
        public JoinedRunViewModelDto(JoinedRun privateRun)
        {
            RunId = privateRun.RunId;
            JoinedRunId = privateRun.JoinedRunId;
            InvitedDate = privateRun.InvitedDate;
            ProfileId = privateRun.ProfileId;
            Status = privateRun.Status;
            Type = privateRun.Type;
            Present = privateRun.Present;
            SquadId = privateRun.SquadId;
            

        }

        public string? JoinedRunId { get; set; }

        /// <summary>
        /// Gets or sets the profile ID of the player
        /// </summary>
        public string? ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the run ID
        /// </summary>
        public string? RunId { get; set; }

        /// <summary>
        /// Gets or sets the date when the player was invited
        /// </summary>
        public DateTime? InvitedDate { get; set; }

        /// <summary>
        /// Gets or sets the acceptance status of the invite
        /// Valid values: "Accepted", "Declined", "Undecided", "Accepted / Pending", "Refund"
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the joined run
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player is present at the run
        /// </summary>
        public bool? Present { get; set; }

        /// <summary>
        /// Gets or sets the squad ID if the player is part of a squad
        /// </summary>
        public string? SquadId { get; set; }
        public Court Court { get; set; }
        public IList<JoinedRun> JoinedRunList { get; set; }
        [NotMapped]
        public string? ImageUrl { get; }
    
    }
}
