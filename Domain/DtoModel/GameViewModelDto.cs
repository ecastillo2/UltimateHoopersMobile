using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class GameViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public GameViewModelDto() { }

        public string GameId { get; set; }
        public string CourtId { get; set; }
        public string RunId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string RunNumber { get; set; }
        public string GameNumber { get; set; }
        public string Status { get; set; }


        public GameViewModelDto(Game game)
        {
            GameId = game.GameId;
            CourtId = game.CourtId;
            RunId = game.RunId;
            CreatedDate = game.CreatedDate;
            RunNumber = game.RunNumber;
            GameNumber = game.GameNumber;
            Status = game.Status;
        }
    }
}
