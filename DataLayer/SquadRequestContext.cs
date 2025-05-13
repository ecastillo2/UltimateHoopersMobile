using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class SquadRequestContext : DbContext
    {
        public SquadRequestContext(DbContextOptions<SquadRequestContext> options) : base(options)
		{

		}

        public DbSet<SquadRequest> SquadRequest { get; set; }
    }
}
