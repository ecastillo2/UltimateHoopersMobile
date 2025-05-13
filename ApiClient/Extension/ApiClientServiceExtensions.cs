using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using WebAPI.ApiClients;

namespace WebAPI.Extensions
{
    /// <summary>
    /// Extension methods for registering API clients
    /// </summary>
    public static class ApiClientServiceExtensions
    {
        /// <summary>
        /// Adds API clients to the service collection
        /// </summary>
        public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            // Register HTTP clients with typed clients
            services.AddHttpClient<IPostApi, PostApi>(client =>
            {
                // Configure default client settings
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://api.undergroundhoopers.com");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register other API clients as needed

            return services;
        }
    }
}