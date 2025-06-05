using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL.Context
{
    public class SubscriptionContext : DbContext
    {
        public SubscriptionContext(DbContextOptions<SubscriptionContext> options) : base(options)
		{

		}
        public DbSet<Subscription> Subscription { get; set; }
    }
}
