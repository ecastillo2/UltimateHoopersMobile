using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class GameDetailViewModelDto : GameViewModelDto
    {
        public string GameId { get; set; }
        public string GameNumber { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RunId { get; set; }
        public string CourtId { get; set; }
        public string ClientId { get; set; }
        public List<Profile> ProfileList { get; set; } = new List<Profile>();
        public Run Run { get; set; }
        public Court Court { get; set; }
        public Game Game { get; set; }
        public IList<Game> GameList { get; set; }
       


    }
}
