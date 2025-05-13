using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    /// <summary>
    /// Database context for error exceptions
    /// </summary>
    public class ErrorExceptionContext : DbContext
    {
        public ErrorExceptionContext(DbContextOptions<ErrorExceptionContext> options) : base(options)
        {
        }

        public DbSet<ErrorException> ErrorExceptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ErrorException entity
            modelBuilder.Entity<ErrorException>()
                .HasKey(e => e.ErrorExceptionId);

            modelBuilder.Entity<ErrorException>()
                .Property(e => e.ErrorMessage)
                .IsRequired();

            modelBuilder.Entity<ErrorException>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}