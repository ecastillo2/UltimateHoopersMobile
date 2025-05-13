using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class PostCommentContext : DbContext
    {
        public PostCommentContext(DbContextOptions<PostCommentContext> options) : base(options)
		{

		}

        public DbSet<PostComment> PostComment { get; set; }
    }
}
