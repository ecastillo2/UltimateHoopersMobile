using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class GameViewModelDto
    {
        public string GameId { get; set; }
        public string CourtId { get; set; }
        public string RunId { get; set; }
        public string CreatedDate { get; set; }
        public string WinProfileIdsStatusString { get; set; }
        public string LoseProfileIdsStatusString { get; set; }
        public string RunNumber { get; set; }
        public string Location { get; set; }
        public string GameNumber { get; set; }
        public string Status { get; set; }
        public string UserWinOrLose { get; set; }

        public GameViewModelDto(Game game)
        {
            GameId = game.GameId;
            CourtId = game.CourtId;
            RunId = game.RunId;
            CreatedDate = game.CreatedDate;
            WinProfileIdsStatusString = game.WinProfileIdsStatusString;
            LoseProfileIdsStatusString = game.LoseProfileIdsStatusString;
            RunNumber = game.RunNumber;
            Location = game.Location;
            GameNumber = game.GameNumber;
            Status = game.Status;
            UserWinOrLose = game.UserWinOrLose;
        }
    }
}
