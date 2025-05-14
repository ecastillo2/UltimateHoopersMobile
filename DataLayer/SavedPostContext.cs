using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class SavedPostContext : DbContext
    {
        public SavedPostContext(DbContextOptions<SavedPostContext> options) : base(options)
		{

		}

        public DbSet<SavedPost> SavedPost { get; set; }
    }
}
