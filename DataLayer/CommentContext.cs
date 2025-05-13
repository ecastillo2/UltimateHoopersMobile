using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class CommentContext : DbContext
    {
        public CommentContext(DbContextOptions<CommentContext> options) : base(options)
		{

		}

        public DbSet<Comment> Comment { get; set; }
    }
}
