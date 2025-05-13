using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class CourtContext : DbContext
    {
        public CourtContext(DbContextOptions<CourtContext> options) : base(options)
		{

		}
        public DbSet<Court> Court { get; set; }
    }
}
