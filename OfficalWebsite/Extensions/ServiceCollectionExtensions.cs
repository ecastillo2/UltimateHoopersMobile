using ApiClient.Authentication;
using ApiClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UltimateHoopers.Services;
using WebAPI.ApiClients;

namespace UltimateHoopers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all API client services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            services.Configure<ApiClientOptions>(configuration.GetSection(ApiClientOptions.SectionName));

            // Register HTTP clients
            services.AddHttpClient<IAuthenticateUser, AuthenticateUser>();
            services.AddHttpClient<IRunApi, RunApi>();
            services.AddHttpClient<IProfileApi, ProfileApi>();
            services.AddHttpClient<IPostApi, PostApi>();
            services.AddHttpClient<IJoinedRunApi, JoinedRunApi>();
            services.AddHttpClient<DashboardApiService>();

            return services;
        }

        /// <summary>
        /// Adds dashboard-specific services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDashboardServices(this IServiceCollection services)
        {
            // Add custom services here
            services.AddScoped<DashboardApiService>();

            return services;
        }
    }
}