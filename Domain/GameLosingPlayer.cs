using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class GameLosingPlayer
    {
        [Key]
        public string? GameLosingPlayerId { get; set; }
        public string? GameId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        
    }
}
