using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ProfileContext : DbContext
    {
        public ProfileContext(DbContextOptions<ProfileContext> options) : base(options)
		{

		}

        public DbSet<Profile> Profile { get; set; }
    }
}
