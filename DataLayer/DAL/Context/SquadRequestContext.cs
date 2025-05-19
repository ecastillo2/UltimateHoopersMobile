using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class SquadRequestContext : DbContext
    {
        public SquadRequestContext(DbContextOptions<SquadRequestContext> options) : base(options)
		{

		}

        public DbSet<SquadRequest> SquadRequest { get; set; }
    }
}
