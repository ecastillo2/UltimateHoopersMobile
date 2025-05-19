using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class ProjectManagementContext : DbContext
    {
        public ProjectManagementContext(DbContextOptions<ProjectManagementContext> options) : base(options)
		{

		}

        public DbSet<ProjectManagement> ProjectManagement { get; set; }
    }
}
