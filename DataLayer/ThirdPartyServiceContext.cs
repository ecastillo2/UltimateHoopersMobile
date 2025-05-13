using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ThirdPartyServiceContext : DbContext
    {
        public ThirdPartyServiceContext(DbContextOptions<ThirdPartyServiceContext> options) : base(options)
		{

		}

        public DbSet<ThirdPartyService> ThirdPartyService { get; set; }
    }
}
