using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class StatusUpdateTimeContext : DbContext
    {
        public StatusUpdateTimeContext(DbContextOptions<StatusUpdateTimeContext> options) : base(options)
		{

		}

        public DbSet<StatusUpdateTime> StatusUpdateTime { get; set; }
    }
}
