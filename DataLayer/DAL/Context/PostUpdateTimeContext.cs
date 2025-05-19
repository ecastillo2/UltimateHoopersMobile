using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class PostUpdateTimeContext : DbContext
    {
        public PostUpdateTimeContext(DbContextOptions<PostUpdateTimeContext> options) : base(options)
		{

		}

        public DbSet<PostUpdateTime> PostUpdateTime { get; set; }
    }
}
