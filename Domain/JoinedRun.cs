using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    public class JoinedRun
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public JoinedRun() { }

        // Existing constructor for mapping from ScoutingReport
        public JoinedRun(List<Profile> profiles)
        {
            JoinedRunProfiles = profiles;
        }

        [Key]
        public string? JoinedRunId { get; set; }
        public string? ProfileId { get; set; }
        public string? RunId { get; set; }       
        public string? InvitedDate { get; set; }
        public string? AcceptedInvite { get; set; }
        public string? Type { get; set; }
        public bool? Present { get; set; }
        public string? SquadId { get; set; }


        [NotMapped]
        public List<Profile>? JoinedRunProfiles { get; set; }
    }
}
