using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class RatePlayerContext : DbContext
    {
        public RatePlayerContext(DbContextOptions<RatePlayerContext> options) : base(options)
		{

		}

        public DbSet<RatePlayer> RatePlayer { get; set; }
    }
}
