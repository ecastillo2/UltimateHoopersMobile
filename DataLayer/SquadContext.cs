using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class SquadContext : DbContext
    {
        public SquadContext(DbContextOptions<SquadContext> options) : base(options)
		{

		}

        public DbSet<Squad> Squad { get; set; }
    }
}
