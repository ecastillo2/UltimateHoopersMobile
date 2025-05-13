using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Squad
    {
        [Key]
        public string? SquadId { get; set; }
        public string? OwnerProfileId { get; set; }
        public string? Name { get; set; }
        [NotMapped]
        public SquadTeam SquadTeam { get; set; }
        [NotMapped]
        public List<SquadRequest>? SquadRequest { get; set; }
    }
}
