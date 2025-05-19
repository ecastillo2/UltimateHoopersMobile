using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class SquadContext : DbContext
    {
        public SquadContext(DbContextOptions<SquadContext> options) : base(options)
		{

		}

        public DbSet<Squad> Squad { get; set; }
    }
}
