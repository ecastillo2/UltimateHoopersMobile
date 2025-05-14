// App.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Pages;
using UltimateHoopers.Services;

namespace UltimateHoopers
{
    public partial class App : Application
    {
        // Auth token for global access
        public static string AuthToken { get; set; }

        public App()
        {
            InitializeComponent();

            // Check for existing authentication
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;

                // Get auth service
                var authService = serviceProvider.GetService<IAuthService>();

                if (authService != null)
                {
                    try
                    {
                        // Check if user is authenticated
                        bool isAuthenticated = await authService.IsAuthenticatedAsync();

                        if (isAuthenticated)
                        {
                            // Get token and set it for global access
                            AuthToken = await authService.GetTokenAsync();

                            // Try to get AppShell from DI
                            var appShell = serviceProvider.GetService<AppShell>();

                            if (appShell != null)
                            {
                                MainPage = appShell;
                            }
                            else
                            {
                                // Fallback if DI can't resolve the AppShell
                                MainPage = new AppShell(authService);
                            }
                        }
                        else
                        {
                            // User is not authenticated, show login page
                            SetLoginPage(serviceProvider, authService);
                        }
                    }
                    catch (Exception authEx)
                    {
                        // Handle authentication errors
                        System.Diagnostics.Debug.WriteLine($"Authentication error: {authEx.Message}");
                        SetLoginPage(serviceProvider, authService);
                    }
                }
                else
                {
                    // Fallback to LoginPage without auth service
                    MainPage = new LoginPage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing app: {ex.Message}");
                // Fallback to login page without auth service
                MainPage = new LoginPage();
            }
        }

        private void SetLoginPage(IServiceProvider serviceProvider, IAuthService authService)
        {
            // Try to get LoginPage from DI
            var loginPage = serviceProvider.GetService<LoginPage>();

            if (loginPage != null)
            {
                MainPage = loginPage;
            }
            else if (authService != null)
            {
                // Fallback if DI can't resolve the LoginPage but we have authService
                MainPage = new LoginPage(authService);
            }
            else
            {
                // Fallback if both DI resolution and authService fail
                MainPage = new LoginPage();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}