using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    public class PrivateRunInvite
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public PrivateRunInvite() { }

        // Existing constructor for mapping from ScoutingReport
        public PrivateRunInvite(List<Profile> profiles)
        {
            InvitedProfiles = profiles;
           
            

        }

        [Key]
        public string? PrivateRunInviteId { get; set; }
        public string? ProfileId { get; set; }
        public string? PrivateRunId { get; set; }       
        public string? InvitedDate { get; set; }
        public string? AcceptedInvite { get; set; }
        public string? Type { get; set; }
        public bool? Present { get; set; }
        public string? SquadId { get; set; }

        

        [NotMapped]
        public List<Profile>? InvitedProfiles { get; set; }
    }
}
