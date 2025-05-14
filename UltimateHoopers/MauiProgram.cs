using Microsoft.Extensions.Logging;
using UltimateHoopers.Pages;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using ApiClient.Authentication;
using CommunityToolkit.Maui;

namespace UltimateHoopers
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // Add Community Toolkit
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Create a configuration object for API client settings
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ApiSettings:BaseUrl"] = "https://ultimatehoopersapi.azurewebsites.net/"
                })
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);

            // Register HTTP client
            builder.Services.AddSingleton<HttpClient>();

            // Register services
            builder.Services.AddSingleton<IPostService, PostService>();
            builder.Services.AddSingleton<IAuthenticateUser, AuthenticateUser>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // Register ViewModels
            builder.Services.AddTransient<PostsViewModel>();

            // Register shell
            builder.Services.AddTransient<AppShell>();

            // Register pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<PostsPage>();
            builder.Services.AddTransient<LoginPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}