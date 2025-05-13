using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataLayer
{
    /// <summary>
    /// Extension methods for setting up DataLayer services in an IServiceCollection
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds DataLayer services to the specified IServiceCollection
        /// </summary>
        public static IServiceCollection AddDataLayer(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory = null)
        {
            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration.GetConnectionString("UnderGroundhoopersDB");
            }

            // Configure DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
               

                // Disable change tracking for read-only scenarios
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                // Enable sensitive data logging only in development
#if DEBUG
                options.EnableSensitiveDataLogging();
#endif

                // Configure logging if provided
                if (loggerFactory != null)
                {
                    options.UseLoggerFactory(loggerFactory);
                }
            });

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostCommentRepository, PostCommentRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IFollowingRepository, FollowingRepository>();
            services.AddScoped<IFollowerRepository, FollowerRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IPrivateRunRepository, PrivateRunRepository>();
            services.AddScoped<IPrivateRunInviteRepository, PrivateRunInviteRepository>();
            services.AddScoped<ICourtRepository, CourtRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IErrorExceptionRepository, ErrorExceptionRepository>();

            // Add more repositories as needed

            return services;
        }
    }
}