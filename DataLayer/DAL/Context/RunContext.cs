using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class RunContext : DbContext
    {
        public RunContext(DbContextOptions<RunContext> options) : base(options)
		{

		}

        public DbSet<Run> Run { get; set; }
    }
}
