using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    public class SquadTeam
    {
        [Key]
        public string? SquadTeamId { get; set; }
        public string? SquadId { get; set; }
        public string? ProfileId { get; set; }
        public bool? RequestResponse { get; set; }

        // FIX: This should be List<Profile>, not List<string>
        [NotMapped]
        [JsonIgnore]
        public List<Profile>? ProfileList { get; set; }

        [NotMapped]
        [JsonIgnore]
        public Profile? Owner { get; set; }

        [NotMapped]
        public string? Name { get; set; }

    }
}
