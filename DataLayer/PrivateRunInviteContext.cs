using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class PrivateRunInviteContext : DbContext
    {
        public PrivateRunInviteContext(DbContextOptions<PrivateRunInviteContext> options) : base(options)
		{

		}

        public DbSet<PrivateRunInvite> PrivateRunInvite { get; set; }
    }
}
