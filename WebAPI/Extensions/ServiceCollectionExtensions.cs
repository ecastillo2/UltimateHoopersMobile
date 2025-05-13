using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using WebAPI.Extensions;

namespace WebAPI.Extensions
{
    /// <summary>
    /// Extension methods for configuring services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures database contexts and repositories
        /// </summary>
        public static IServiceCollection AddDataServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Get the connection string from configuration
            var connectionString = configuration.GetConnectionString("UnderGroundhoopersDB");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string 'UnderGroundhoopersDB' is not configured");
            }

            // Configure main database context
            services.AddDbContext<HUDBContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Configure options for better SQL Server performance
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                }));

            // Configure other contexts if needed
            services.AddDbContext<ErrorExceptionContext>(options =>
                options.UseSqlServer(connectionString));

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

            return services;
        }

        /// <summary>
        /// Configures common utilities and services
        /// </summary>
        public static IServiceCollection AddCommonServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register common utility services
            services.AddScoped<Common.VideoProcessing.VideoConverter>(sp =>
            {
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Common.VideoProcessing.VideoConverter>>();
                var ffmpegPath = configuration["FFmpegPath"];
                return new Common.VideoProcessing.VideoConverter(logger, ffmpegPath);
            });

            // Add memory caching
            services.AddMemoryCache();

            // Add response caching
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024 * 1024; // 1MB
                options.UseCaseSensitivePaths = false;
            });

            return services;
        }

        /// <summary>
        /// Configure authentication and authorization
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}

