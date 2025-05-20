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
                System.Diagnostics.Debug.WriteLine("Setting initial page to Posts page");

                if (shell == null || shell.Items == null || shell.Items.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Shell or shell items is null or empty");
                    return;
                }

                // Log some debug info about shell items
                foreach (var item in shell.Items)
                {
                    if (item != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Shell Item: Route={item.Route}, Items.Count={item.Items?.Count ?? 0}");

                        if (item.Items != null)
                        {
                            foreach (var subItem in item.Items)
                            {
                                if (subItem != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  - ShellContent: Route={subItem.Route}");
                                }
                            }
                        }
                    }
                }

                // Check if we're already on the PostsPage - if so, do nothing
                if (shell.CurrentItem != null &&
                    (shell.CurrentItem.Route?.Equals("PostsPage", StringComparison.OrdinalIgnoreCase) == true ||
                     shell.CurrentItem.CurrentItem?.Route?.Equals("PostsPage", StringComparison.OrdinalIgnoreCase) == true))
                {
                    System.Diagnostics.Debug.WriteLine("Already on Posts page, no navigation needed");
                    return;
                }

                // First look for FlyoutItem with Route="PostsPage"
                var postsItem = shell.Items.FirstOrDefault(item =>
                    item != null &&
                    item.Route != null &&
                    item.Route.Equals("PostsPage", StringComparison.OrdinalIgnoreCase));

                if (postsItem != null)
                {
                    System.Diagnostics.Debug.WriteLine("Found posts item by route, setting as current item");
                    shell.CurrentItem = postsItem;
                    return;
                }

                // If we didn't find a direct match, look for a ShellContent with Route="PostsPage"
                foreach (var item in shell.Items)
                {
                    if (item?.Items != null)
                    {
                        var postsContent = item.Items.FirstOrDefault(sc =>
                            sc != null &&
                            sc.Route != null &&
                            sc.Route.Equals("PostsPage", StringComparison.OrdinalIgnoreCase));

                        if (postsContent != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found posts content in {item.Route}, setting as current");
                            shell.CurrentItem = item;
                            item.CurrentItem = postsContent;
                            return;
                        }
                    }
                }

                // If direct lookup fails, try using GoToAsync
                try
                {
                    System.Diagnostics.Debug.WriteLine("Attempting to navigate using GoToAsync");
                    MainThread.BeginInvokeOnMainThread(async () => {
                        try
                        {
                            await Shell.Current.GoToAsync("//PostsPage");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"GoToAsync navigation error: {ex.Message}");
                        }
                    });
                    return;
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");
                }

                System.Diagnostics.Debug.WriteLine("Could not find Posts page in shell items");
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app if setting initial page fails
                System.Diagnostics.Debug.WriteLine($"Error setting initial page to Posts: {ex.Message}");
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

                            // Create AppShell
                            AppShell appShell = null;

                            // Try to get AppShell from DI
                            appShell = serviceProvider.GetService<AppShell>();
                            DiagnosticHelper.Log($"AppShell obtained from DI: {(appShell != null ? "Yes" : "No")}");

                            if (appShell == null)
                            {
                                // Create new shell if DI failed
                                appShell = new AppShell(authService);
                                DiagnosticHelper.Log("Created new AppShell with auth service");
                            }

                            // Set the main page to the shell - using try/catch for safety
                            try
                            {
                                MainPage = appShell;
                                DiagnosticHelper.Log("Set MainPage to AppShell");
                            }
                            catch (Exception shellEx)
                            {
                                DiagnosticHelper.LogException(shellEx, "Setting MainPage to AppShell");
                                // Fallback to creating a new AppShell
                                try
                                {
                                    MainPage = new AppShell(authService);
                                    DiagnosticHelper.Log("Set MainPage to new AppShell (fallback)");
                                }
                                catch (Exception fallbackEx)
                                {
                                    DiagnosticHelper.LogException(fallbackEx, "Setting MainPage to fallback AppShell");
                                    // Last resort - go to login page
                                    SetLoginPage(serviceProvider, authService);
                                    return;
                                }
                            }

                            // Navigate to PostsPage after a short delay to ensure shell is initialized
                            await Task.Delay(300);  // Increase delay for better initialization

                            // Safely set the Posts page as the current page
                            try
                            {
                                SetInitialPage(appShell);
                            }
                            catch (Exception navEx)
                            {
                                DiagnosticHelper.LogException(navEx, "Setting initial page");
                                // Continue anyway - the shell will attempt to navigate
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