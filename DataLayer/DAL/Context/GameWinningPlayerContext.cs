using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class GameWinningPlayerContext : DbContext
    {
        public GameWinningPlayerContext(DbContextOptions<GameWinningPlayerContext> options) : base(options)
		{

		}

        public DbSet<GameWinningPlayer> GameWinningPlayer { get; set; }
    }
}
