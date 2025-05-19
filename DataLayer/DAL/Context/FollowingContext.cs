using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class FollowingContext : DbContext
    {
        public FollowingContext(DbContextOptions<FollowingContext> options) : base(options)
		{

		}

        public DbSet<Following> Following { get; set; }
    }
}
