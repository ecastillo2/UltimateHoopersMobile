using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class CourtContext : DbContext
    {
        public CourtContext(DbContextOptions<CourtContext> options) : base(options)
		{

		}
        public DbSet<Court> Court { get; set; }
    }
}
