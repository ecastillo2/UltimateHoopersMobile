using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class GameDetailViewModelDto : GameViewModelDto
    {
        public Game Game { get; set; }
        public IList<Game> GameList { get; set; }
       


    }
}
