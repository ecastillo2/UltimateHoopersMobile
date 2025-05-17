using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class PrivateRunDetailViewModelDto : PrivateRunViewModelDto
    {
       
        public string FollowersCount { get; set; }
        public string FollowingCount { get; set; }

      
    }
}
