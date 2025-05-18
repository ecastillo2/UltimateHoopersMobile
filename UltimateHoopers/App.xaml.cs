using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Pages;
using UltimateHoopers.Services;
using UltimateHoopers.Helpers;
using Microsoft.Maui.Dispatching;
using Domain;

namespace UltimateHoopers
{
    public partial class App : Application
    {
        // Auth token for global access
        public static string AuthToken { get; set; }
        public static Profile Profile { get; set; }
        public static User User { get; set; }

        public App()
        {
            try
            {
                DiagnosticHelper.Log("App constructor starting");
                InitializeComponent();
                DiagnosticHelper.Log("InitializeComponent complete");
               

                // Check for existing authentication
                InitializeAsync();
                // Show splash screen on startup
                // Use code-only splash screen 
                MainPage = new CodeSplashScreen();
                DiagnosticHelper.Log("InitializeAsync called");
            }
            catch (Exception ex)
            {
                DiagnosticHelper.LogException(ex, "App constructor");
                MainPage = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = "Startup Error",
                                FontSize = 22,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = ex.Message,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            }
                        },
                        VerticalOptions = LayoutOptions.Center,
                        Padding = new Thickness(20)
                    }
                };
            }
        }

        private void SetInitialPage(Shell shell)
        {
            try
            {
                if (shell == null || shell.Items == null || shell.Items.Count == 0)
                    return;

                // First look for a matching tab/flyout item with the exact route
                var postsItem = shell.Items.FirstOrDefault(item =>
                    item != null &&
                    "PostsPage".Equals(item.Route, StringComparison.OrdinalIgnoreCase));

                if (postsItem != null)
                {
                    shell.CurrentItem = postsItem;
                    DiagnosticHelper.Log("Set shell.CurrentItem to PostsPage directly");
                    return;
                }

                // Then look for an item containing a tab with the route
                foreach (var item in shell.Items)
                {
                    if (item?.Items != null)
                    {
                        var tab = item.Items.FirstOrDefault(si =>
                            si != null &&
                            "PostsPage".Equals(si.Route, StringComparison.OrdinalIgnoreCase));

                        if (tab != null)
                        {
                            shell.CurrentItem = item;
                            if (item.CurrentItem != tab)
                            {
                                item.CurrentItem = tab;
                            }
                            DiagnosticHelper.Log("Set shell.CurrentItem to item containing PostsPage tab");
                            return;
                        }
                    }
                }

                // If we didn't find a Posts page specifically, try to find any page that has "Posts" in its name
                foreach (var item in shell.Items)
                {
                    if (item?.Route != null && item.Route.Contains("Posts", StringComparison.OrdinalIgnoreCase))
                    {
                        shell.CurrentItem = item;
                        DiagnosticHelper.Log($"Set shell.CurrentItem to item with route containing 'Posts': {item.Route}");
                        return;
                    }

                    if (item?.Items != null)
                    {
                        var tab = item.Items.FirstOrDefault(si =>
                            si?.Route != null &&
                            si.Route.Contains("Posts", StringComparison.OrdinalIgnoreCase));

                        if (tab != null)
                        {
                            shell.CurrentItem = item;
                            if (item.CurrentItem != tab)
                            {
                                item.CurrentItem = tab;
                            }
                            DiagnosticHelper.Log($"Set shell.CurrentItem to item containing tab with route containing 'Posts': {tab.Route}");
                            return;
                        }
                    }
                }

                DiagnosticHelper.Log("Could not find Posts page in shell items");
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app if setting initial page fails
                DiagnosticHelper.LogException(ex, "SetInitialPage");
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                DiagnosticHelper.Log("InitializeAsync starting");

                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                DiagnosticHelper.Log("Service provider obtained");

                // Get auth service
                var authService = serviceProvider.GetService<IAuthService>();
                DiagnosticHelper.Log($"Auth service obtained: {(authService != null ? "Yes" : "No")}");

                if (authService != null)
                {
                    try
                    {
                        DiagnosticHelper.Log("Checking authentication");
                        // Check if user is authenticated
                        bool isAuthenticated = await authService.IsAuthenticatedAsync();
                        DiagnosticHelper.Log($"Is authenticated: {isAuthenticated}");

                        if (isAuthenticated)
                        {
                            // Get token and set it for global access
                            AuthToken = await authService.GetTokenAsync();
                            DiagnosticHelper.Log("Auth token retrieved");

                            // Try to get AppShell from DI
                            var appShell = serviceProvider.GetService<AppShell>();
                            DiagnosticHelper.Log($"AppShell obtained from DI: {(appShell != null ? "Yes" : "No")}");

                            if (appShell != null)
                            {
                                // Set the main page to the shell
                                MainPage = appShell;
                                DiagnosticHelper.Log("Set MainPage to AppShell from DI");

                                // Safely set the Posts page as the current page
                                SetInitialPage(appShell);
                            }
                            else
                            {
                                // Fallback if DI can't resolve the AppShell
                                DiagnosticHelper.Log("Creating new AppShell with auth service");
                                var shell = new AppShell(authService);
                                MainPage = shell;
                                DiagnosticHelper.Log("Set MainPage to new AppShell");

                                // Safely set the Posts page as the current page
                                SetInitialPage(shell);
                            }
                        }
                        else
                        {
                            // User is not authenticated, show login page
                            DiagnosticHelper.Log("User not authenticated, showing login page");
                            SetLoginPage(serviceProvider, authService);
                        }
                    }
                    catch (Exception authEx)
                    {
                        // Handle authentication errors
                        DiagnosticHelper.LogException(authEx, "Authentication");
                        System.Diagnostics.Debug.WriteLine($"Authentication error: {authEx.Message}");
                        SetLoginPage(serviceProvider, authService);
                    }
                }
                else
                {
                    // Fallback to LoginPage without auth service
                    DiagnosticHelper.Log("Auth service not available, creating LoginPage");
                    MainPage = new LoginPage();
                    DiagnosticHelper.Log("Set MainPage to LoginPage");
                }
            }
            catch (Exception ex)
            {
                DiagnosticHelper.LogException(ex, "InitializeAsync");
                System.Diagnostics.Debug.WriteLine($"Error initializing app: {ex.Message}");
                // Fallback to login page without auth service
                MainPage = new LoginPage();
                DiagnosticHelper.Log("Error fallback: Set MainPage to LoginPage");
            }
        }

        private void SetLoginPage(IServiceProvider serviceProvider, IAuthService authService)
        {
            try
            {
                DiagnosticHelper.Log("Setting login page");
                // Try to get LoginPage from DI
                var loginPage = serviceProvider.GetService<LoginPage>();
                DiagnosticHelper.Log($"LoginPage obtained from DI: {(loginPage != null ? "Yes" : "No")}");

                if (loginPage != null)
                {
                    MainPage = loginPage;
                    DiagnosticHelper.Log("Set MainPage to LoginPage from DI");
                }
                else if (authService != null)
                {
                    // Fallback if DI can't resolve the LoginPage but we have authService
                    DiagnosticHelper.Log("Creating new LoginPage with auth service");
                    MainPage = new LoginPage(authService);
                    DiagnosticHelper.Log("Set MainPage to new LoginPage with auth service");
                }
                else
                {
                    // Fallback if both DI resolution and authService fail
                    DiagnosticHelper.Log("Creating new LoginPage without auth service");
                    MainPage = new LoginPage();
                    DiagnosticHelper.Log("Set MainPage to new LoginPage without auth service");
                }
            }
            catch (Exception ex)
            {
                DiagnosticHelper.LogException(ex, "SetLoginPage");
                // Last resort fallback
                MainPage = new LoginPage();
                DiagnosticHelper.Log("Error fallback: Set MainPage to LoginPage");
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            DiagnosticHelper.Log("OnStart called");
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            DiagnosticHelper.Log("OnSleep called");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            DiagnosticHelper.Log("OnResume called");
        }
    }
}