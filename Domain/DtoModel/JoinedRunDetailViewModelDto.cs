using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class JoinedRunDetailViewModelDto : JoinedRunViewModelDto
    {
        public JoinedRun JoinedRun { get; set; }
        public Run Run { get; set; }
      

    }
}
