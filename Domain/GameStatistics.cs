using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class GameStatistics
    {
    
        public int? TotalGames { get; set; }
        public double? WinPercentage { get; set; }
        public int? TotalWins { get; set; }
        public int? TotalLosses { get; set; }
    }
}
