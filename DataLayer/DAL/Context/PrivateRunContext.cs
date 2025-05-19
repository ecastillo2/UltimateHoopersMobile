using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class PrivateRunContext : DbContext
    {
        public PrivateRunContext(DbContextOptions<PrivateRunContext> options) : base(options)
		{

		}

        public DbSet<PrivateRun> PrivateRun { get; set; }
    }
}
