using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class LikedPostContext : DbContext
    {
        public LikedPostContext(DbContextOptions<LikedPostContext> options) : base(options)
		{

		}

        public DbSet<LikedPost> LikedPost { get; set; }
    }
}
