using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ContactContext : DbContext
    {
        public ContactContext(DbContextOptions<ContactContext> options) : base(options)
		{

		}
        public DbSet<Contact> Contact { get; set; }
    }
}
