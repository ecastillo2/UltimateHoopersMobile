using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ProfileDetailViewModelDto : ProfileViewModelDto
    {
        public Profile? Profile { get; set; }
        public Setting? Setting { get; set; }
        public Subscription? Subscription { get; set; }
        public ScoutingReport? ScoutingReport { get; set; }
        public GameStatistics? GameStatistics { get; set; }
       
    }
}
