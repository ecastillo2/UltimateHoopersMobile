using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class PlayerCommentContext : DbContext
    {
        public PlayerCommentContext(DbContextOptions<PlayerCommentContext> options) : base(options)
		{

		}

        public DbSet<PlayerComment> PlayerComment { get; set; }
    }
}
