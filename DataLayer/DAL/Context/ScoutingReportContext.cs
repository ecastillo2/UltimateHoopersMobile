using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class ScoutingReportContext : DbContext
    {
        public ScoutingReportContext(DbContextOptions<ScoutingReportContext> options) : base(options)
		{

		}

        public DbSet<ScoutingReport> ScoutingReport { get; set; }
    }
}
