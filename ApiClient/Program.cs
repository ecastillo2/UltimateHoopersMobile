using ApiClient.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Create a host builder
var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Add logging
        services.AddLogging();

        // Add API clients
        services.AddApiClients(hostContext.Configuration);

        // Add your application services here
    });

// Build and run the host
using var host = hostBuilder.Build();
await host.RunAsync();