using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class SquadTeam
    {
        [Key]
        public string? SquadTeamId { get; set; }
        public string? SquadId { get; set; }
        public string? ProfileId { get; set; }
        public bool? RequestResponse { get; set; }
        [NotMapped]
        public List<Profile>? ProfileList { get; set; }


        [NotMapped]
        public Profile? Owner { get; set; }
        [NotMapped]
        public string? Name { get; set; }

    }
}
