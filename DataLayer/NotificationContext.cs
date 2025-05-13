using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class NotificationContext : DbContext
    {
        public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
		{

		}

        public DbSet<Notification> Notification { get; set; }
    }
}
