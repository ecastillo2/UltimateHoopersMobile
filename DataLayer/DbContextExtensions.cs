using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataLayer
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Configures the DbContext for optimal performance in production
        /// </summary>
        public static IServiceCollection OptimizeDbContextForProduction(
            this IServiceCollection services,
            string connectionString,
            ILoggerFactory loggerFactory = null)
        {
            services.AddDbContext<HUDBContext>(options =>
            {
                // Configure SQL Server with performance optimizations
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Enable connection resiliency
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    // Optimize data loading with batching
                    sqlOptions.MaxBatchSize(100);

                    // Set command timeout
                    sqlOptions.CommandTimeout(30);
                });

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

            return services;
        }

        /// <summary>
        /// Executes a raw SQL query with proper parameterization
        /// </summary>
        public static async Task<List<T>> ExecuteRawSqlQueryAsync<T>(
            this DbContext context,
            string sql,
            params object[] parameters)
            where T : class
        {
            return await context.Set<T>()
                .FromSqlRaw(sql, parameters)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}