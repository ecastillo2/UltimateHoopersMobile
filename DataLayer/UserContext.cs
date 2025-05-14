using Microsoft.EntityFrameworkCore;
using Domain;
using System.Collections.Generic;

namespace DataLayer
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
		{
		}

        public DbSet<User> User { get; set; }
    }
}
