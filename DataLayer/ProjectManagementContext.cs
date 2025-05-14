using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ProjectManagementContext : DbContext
    {
        public ProjectManagementContext(DbContextOptions<ProjectManagementContext> options) : base(options)
		{

		}

        public DbSet<ProjectManagement> ProjectManagement { get; set; }
    }
}
