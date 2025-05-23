using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class JoinedRunContext : DbContext
    {
        public JoinedRunContext(DbContextOptions<JoinedRunContext> options) : base(options)
		{

		}

        public DbSet<JoinedRun> JoinedRun { get; set; }
    }
}
