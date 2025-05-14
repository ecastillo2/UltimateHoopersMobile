using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class TagContext : DbContext
    {
        public TagContext(DbContextOptions<TagContext> options) : base(options)
		{

		}

        public DbSet<Tag> Tag { get; set; }
    }
}
