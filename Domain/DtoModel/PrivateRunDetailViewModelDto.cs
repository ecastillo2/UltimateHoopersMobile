using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class PrivateRunDetailViewModelDto : PrivateRunViewModelDto
    {

        public PrivateRun PrivateRun { get; set; }
        public Court Court { get; set; }
        public PrivateRunInvite PrivateRunInvite { get; set; }

        public int? PlayerCount { get; set; }
       

        public PrivateRunDetailViewModelDto(PrivateRun privateRun) : base(privateRun)
        {
            PlayerCount = privateRun.PlayerCount;
          
        }
    }
}
