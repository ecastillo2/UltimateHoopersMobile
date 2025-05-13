using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ActivityContext : DbContext
    {
        public ActivityContext(DbContextOptions<ActivityContext> options) : base(options)
		{

		}

        public DbSet<Activity> Activity { get; set; }
    }
}
