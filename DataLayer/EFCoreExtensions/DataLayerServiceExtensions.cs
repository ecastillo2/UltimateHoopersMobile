using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataLayer
{
    /// <summary>
    /// Extension methods for registering data layer services
    /// </summary>
    public static class DataLayerServiceExtensions
    {
        /// <summary>
        /// Adds DataLayer services to the specified IServiceCollection
        /// </summary>
        public static IServiceCollection AddDataLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration.GetConnectionString("UnderGroundhoopersDB");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string is not configured. Please provide either 'DefaultConnection' or 'UnderGroundhoopersDB' in your connection strings.");
            }

            // Configure DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Configure options for better SQL Server performance
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                }));

            services.AddDbContext<HUDBContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Configure options for better SQL Server performance
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                }));

            services.AddDbContext<ErrorExceptionContext>(options =>
                options.UseSqlServer(connectionString));

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

            return services;
        }
    }
}