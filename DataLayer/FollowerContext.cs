using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class FollowerContext : DbContext
    {
        public FollowerContext(DbContextOptions<FollowerContext> options) : base(options)
		{

		}

        public DbSet<Follower> Follower { get; set; }
    }
}
