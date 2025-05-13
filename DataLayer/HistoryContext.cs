using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class HistoryContext : DbContext
    {
        public HistoryContext(DbContextOptions<HistoryContext> options) : base(options)
		{

		}

        public DbSet<History> History { get; set; }
    }
}
