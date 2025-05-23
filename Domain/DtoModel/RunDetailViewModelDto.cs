using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class RunDetailViewModelDto : RunViewModelDto
    {

        public Run Run { get; set; }
        public Court Court { get; set; }
        public IList<Profile> JoinedRunProfileList { get; set; }

        public int? PlayerCount { get; set; }
       

        public RunDetailViewModelDto(Run run) : base(run)
        {
            PlayerCount = run.PlayerCount;
          
        }
    }
}
