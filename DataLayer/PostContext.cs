using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class PostContext : DbContext
    {
        public PostContext(DbContextOptions<PostContext> options) : base(options)
		{

		}

        public DbSet<Post> Post { get; set; }
    }
}
