using DataLayer.DAL;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;
using DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WebAPI.Services;

namespace WebAPI.Extensions
{
    /// <summary>
    /// Extension methods for service registration
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Add data services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the main DbContext
            services.AddDbContext<HUDBContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("UnderGroundhoopersDB"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)
                ));

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register individual repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<ICourtRepository, CourtRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IPrivateRunRepository, PrivateRunRepository>();
            services.AddScoped<IPrivateRunInviteRepository, PrivateRunInviteRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IFollowerRepository, FollowerRepository>();
            services.AddScoped<IFollowingRepository, FollowingRepository>();
            services.AddScoped<ILikedPostRepository, LikedPostRepository>();
            services.AddScoped<IPlayerCommentRepository, PlayerCommentRepository>();
            services.AddScoped<IPostCommentRepository, PostCommentRepository>();
            services.AddScoped<ISavedPostRepository, SavedPostRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();

            return services;
        }

        /// <summary>
        /// Add application services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register authentication services
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        /// <summary>
        /// Add cross-origin resource sharing (CORS) to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());

                options.AddPolicy("AllowSpecificOrigins", builder =>
                    builder.WithOrigins(
                            "https://ultimatehoopers.com",
                            "https://www.ultimatehoopers.com",
                            "https://app.ultimatehoopers.com",
                            "http://localhost:3000",
                            "http://localhost:5000")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials());
            });

            return services;
        }
    }
}