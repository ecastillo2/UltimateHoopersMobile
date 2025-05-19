using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class CriteriaContext : DbContext
    {
        public CriteriaContext(DbContextOptions<CriteriaContext> options) : base(options)
		{

		}

        public DbSet<Criteria> Criteria { get; set; }
    }
}
