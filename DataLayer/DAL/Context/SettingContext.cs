using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class SettingContext : DbContext
    {
        public SettingContext(DbContextOptions<SettingContext> options) : base(options)
		{

		}

        public DbSet<Setting> Setting { get; set; }
    }
}
