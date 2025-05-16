using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class SquadViewModelDto
    {
        public string SquadId { get; set; }
        public string OwnerProfileId { get; set; }
        public string Name { get; set; }

        public SquadViewModelDto(Squad squad)
        {
            SquadId = squad.SquadId;
            OwnerProfileId = squad.OwnerProfileId;
            Name = squad.Name;
        }
    }
}
