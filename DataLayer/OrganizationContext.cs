using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class OrganizationContext : DbContext
    {
        public OrganizationContext(DbContextOptions<OrganizationContext> options) : base(options)
		{

		}

        public DbSet<Organization> Organization { get; set; }
    }
}
