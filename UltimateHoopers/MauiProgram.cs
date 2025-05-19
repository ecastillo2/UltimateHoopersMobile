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
using UltimateHoopers.Helpers;
using System;
using Domain;
using CommunityToolkit.Maui.Media;

namespace UltimateHoopers
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                DiagnosticHelper.Log("CreateMauiApp starting");

                var builder = MauiApp.CreateBuilder();
                DiagnosticHelper.Log("MauiApp builder created");

                builder
                    .UseMauiApp<App>()
                    .UseMauiCommunityToolkit() // Keep this for existing Community Toolkit features
                    .UseMauiCommunityToolkitMediaElement() // Add this to initialize MediaElement
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                DiagnosticHelper.Log("Basic MAUI app configuration completed");

                // Register custom controls
                builder.Services.AddTransient<Controls.AutoPlayVideoElement>();
                DiagnosticHelper.Log("Custom controls registered");

                // Create a configuration object for API client settings
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ApiSettings:BaseUrl"] = "https://ultimatehoopersapi.azurewebsites.net/"
                    })
                    .Build();

                builder.Services.AddSingleton<IConfiguration>(configuration);
                DiagnosticHelper.Log("Configuration added");

                // Register HTTP client
                builder.Services.AddSingleton<HttpClient>();
                DiagnosticHelper.Log("HttpClient registered");

                // Register services
                builder.Services.AddSingleton<IPostService>(sp =>
                {
                    var httpClient = sp.GetService<HttpClient>();
                    var configuration = sp.GetService<IConfiguration>();
                    var logger = sp.GetService<ILogger<PostService>>();
                    return new PostService(httpClient, configuration, logger);
                });
                builder.Services.AddSingleton<IAuthenticateUser, AuthenticateUser>();
                builder.Services.AddSingleton<IAuthService, AuthService>();
                DiagnosticHelper.Log("Services registered");

                // Register ViewModels
                builder.Services.AddTransient<PostsViewModel>();
                DiagnosticHelper.Log("ViewModels registered");

                // Register shell
                builder.Services.AddTransient<AppShell>();
                DiagnosticHelper.Log("AppShell registered");

                // Register pages
                builder.Services.AddTransient<HomePage>();
                builder.Services.AddTransient<PostsPage>();
                builder.Services.AddTransient<LoginPage>();
                builder.Services.AddTransient<VideoPlayerPage>();
                builder.Services.AddTransient<CodeSplashScreen>();
                builder.Services.AddTransient<HoopersPage>();
                builder.Services.AddTransient<StatsPage>();
                builder.Services.AddTransient<FindRunsPage>();
                builder.Services.AddTransient<ShopPage>();
                builder.Services.AddTransient<EditProfilePage>();
                builder.Services.AddTransient<CreateAccountPage>();
                builder.Services.AddTransient<PlayerProfilePage>();
                builder.Services.AddTransient<PaymentPage>();
                builder.Services.AddTransient<NotificationsPage>();
                builder.Services.AddTransient<NotificationSettingsPage>();
                DiagnosticHelper.Log("Pages registered");

#if DEBUG
                builder.Logging.AddDebug();
                DiagnosticHelper.Log("Debug logging added");
#endif

                var app = builder.Build();
                DiagnosticHelper.Log("MauiApp built successfully");
                return app;
            }
            catch (Exception ex)
            {
                DiagnosticHelper.LogException(ex, "CreateMauiApp");
                throw; // Rethrow as we need to fail the app creation if this fails
            }
        }
    }
}