using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class RatingContext : DbContext
    {
        public RatingContext(DbContextOptions<RatingContext> options) : base(options)
		{

		}

        public DbSet<Rating> Rating { get; set; }
    }
}
