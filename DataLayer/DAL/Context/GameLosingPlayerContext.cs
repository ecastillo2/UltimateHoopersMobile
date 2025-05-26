using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class GameLosingPlayerContext : DbContext
    {
        public GameLosingPlayerContext(DbContextOptions<GameLosingPlayerContext> options) : base(options)
		{

		}

        public DbSet<GameLosingPlayer> GameLosingPlayer { get; set; }
    }
}
