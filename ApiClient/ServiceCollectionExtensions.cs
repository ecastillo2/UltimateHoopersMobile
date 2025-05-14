using System;
using ApiClient.Configuration;
//using ApiClient.Interfaces;
using ApiClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.ApiClients;

namespace ApiClient.Extensions
{
    /// <summary>
    /// Extension methods for setting up API client services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds API client services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            services.Configure<ApiClientOptions>(configuration.GetSection(ApiClientOptions.SectionName));

            // Get API client options for HttpClient configuration
            var options = configuration
                .GetSection(ApiClientOptions.SectionName)
                .Get<ApiClientOptions>();

            if (options == null)
            {
                throw new InvalidOperationException(
                    $"Configuration section '{ApiClientOptions.SectionName}' is missing or invalid");
            }

            // Register HTTP clients with dependency injection
          

            // Register other API clients here
            // services.AddHttpClient<IUserApi, UserApiClient>();
            // services.AddHttpClient<IProfileApi, ProfileApiClient>();
            // etc.

            return services;
        }
    }
}