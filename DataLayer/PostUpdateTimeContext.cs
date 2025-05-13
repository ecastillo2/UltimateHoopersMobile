using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class PostUpdateTimeContext : DbContext
    {
        public PostUpdateTimeContext(DbContextOptions<PostUpdateTimeContext> options) : base(options)
		{

		}

        public DbSet<PostUpdateTime> PostUpdateTime { get; set; }
    }
}
