using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ScoutingReportContext : DbContext
    {
        public ScoutingReportContext(DbContextOptions<ScoutingReportContext> options) : base(options)
		{

		}

        public DbSet<ScoutingReport> ScoutingReport { get; set; }
    }
}
