using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class PushSubscriptionContext : DbContext
    {
        public PushSubscriptionContext(DbContextOptions<PushSubscriptionContext> options) : base(options)
		{

		}

        public DbSet<PushSubscription> PushSubscription { get; set; }
    }
}
