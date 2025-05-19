using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class StatusUpdateTimeContext : DbContext
    {
        public StatusUpdateTimeContext(DbContextOptions<StatusUpdateTimeContext> options) : base(options)
		{

		}

        public DbSet<StatusUpdateTime> StatusUpdateTime { get; set; }
    }
}
