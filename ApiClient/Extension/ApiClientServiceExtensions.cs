using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;
using ApiClient.Authentication;
using System.Net.Http; // Make sure this namespace is imported

namespace ApiClient
{
    /// <summary>
    /// Extension methods for registering API client services
    /// </summary>
    public static class ApiClientServiceExtensions
    {
        /// <summary>
        /// Adds API client services to the service collection
        /// </summary>
        public static IServiceCollection AddApiClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register HttpClient with default configuration
            services.AddHttpClient("ApiClient", client =>
            {
                var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://api.undergroundhoopers.com";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register authentication service
            services.AddScoped<IAuthenticateUser, AuthenticateUser>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("ApiClient");
                var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuthenticateUser>>();

                return new AuthenticateUser(
                    httpClient,
                    configuration,
                    logger);
            });

            // Register other API client services as needed

            return services;
        }
    }
}