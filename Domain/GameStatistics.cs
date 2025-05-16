using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class GameStatistics
    {
        public int TotalGames { get; set; }
        public double WinPercentage { get; set; }
        public string TotalWins { get; set; }
        public string TotalLosses { get; set; }
    }
}
