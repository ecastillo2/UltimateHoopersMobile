using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class UserDetailViewModelDto : UserViewModelDto
    {

        public User User { get; set; }
        public Profile Profile { get; set; }
        public IList<Game> GameList { get; set; }


    }
}
