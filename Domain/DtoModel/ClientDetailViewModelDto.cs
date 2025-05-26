using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ClientDetailViewModelDto : ClientViewModelDto
    {

        public IList<Run> RunList { get; set; }
        public IList<Court> CourtList { get; set; }
        public IList<User> UserList { get; set; }


    }
}
