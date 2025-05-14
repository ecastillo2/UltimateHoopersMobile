using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public class ErrorExceptionContext : DbContext
    {
        public ErrorExceptionContext(DbContextOptions<ErrorExceptionContext> options) : base(options)
        {

        }

        public DbSet<ErrorException> ErrorException { get; set; }
    }
}
