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

                // Enable diagnostic mode for page initialization
                PageInitializationHelper.EnableDiagnosticMode();

                InitializeComponent();
                DiagnosticHelper.Log("InitializeComponent complete");

                // Set up global error handling
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

               

                // Check for existing authentication
                InitializeAsync();

                // Show splash screen on startup
                // Use code-only splash screen 
                var splashScreen = new CodeSplashScreen();

                // Monitor the splash screen's lifecycle
                PageInitializationHelper.MonitorPageLifecycle(splashScreen);

                MainPage = splashScreen;
                DiagnosticHelper.Log("InitializeAsync called");
            }
            catch (Exception ex)
            {
                // Rest of your error handling code...
            }
        }

        // Event handlers for unhandled exceptions
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception
            Exception ex = e.ExceptionObject as Exception;
            DiagnosticHelper.LogException(ex, "Unhandled exception");

            // Since this is an app-domain level exception, we can't really recover
            // Just log it and let the app crash, or try to show an error message if possible
            try
            {
                if (MainThread.IsMainThread)
                {
                    ShowFatalErrorPage(ex);
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        ShowFatalErrorPage(ex);
                    });
                }
            }
            catch
            {
                // If this also fails, there's not much we can do
            }
        }

        // Handler for unobserved task exceptions
        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // Log the exception
            DiagnosticHelper.LogException(e.Exception, "Unobserved task exception");

            // Mark as observed to prevent app termination
            e.SetObserved();

            // Try to recover if possible or show error
            try
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    // If this is a critical error, show a message
                    if (IsCriticalException(e.Exception))
                    {
                        ShowErrorMessage("An unexpected error occurred. Some features may not work correctly.");
                    }
                });
            }
            catch
            {
                // If this also fails, there's not much we can do
            }
        }

        // Helper to determine if an exception is critical
        private bool IsCriticalException(Exception ex)
        {
            // Define which exceptions are considered critical
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is AccessViolationException
                || ex is System.Runtime.InteropServices.SEHException;
        }

        // Helper to show a fatal error page
        private void ShowFatalErrorPage(Exception ex)
        {
            var errorPage = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text = "Fatal Error",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        new Label
                        {
                            Text = "An unexpected error has occurred:",
                            HorizontalOptions = LayoutOptions.Start
                        },
                        new Label
                        {
                            Text = ex.Message,
                            HorizontalOptions = LayoutOptions.Start,
                            Margin = new Thickness(0, 10, 0, 20)
                        },
                        new Button
                        {
                            Text = "Restart App",
                            HorizontalOptions = LayoutOptions.Center,
                            Command = new Command(() => {
                                // Try to restart by showing login page
                                MainPage = new LoginPage();
                            })
                        }
                    },
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(20)
                }
            };

            MainPage = errorPage;
        }

        // Helper to show a non-fatal error message
        private void ShowErrorMessage(string message)
        {
            if (MainPage == null) return;

            MainPage.DisplayAlert("Error", message, "OK");
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

                // Check if we're already on the PostsPage - if so, do nothing
                if (shell.CurrentItem != null &&
                    (shell.CurrentItem.Route?.Equals("PostsPage", StringComparison.OrdinalIgnoreCase) == true ||
                     shell.CurrentItem.CurrentItem?.Route?.Equals("PostsPage", StringComparison.OrdinalIgnoreCase) == true))
                {
                    System.Diagnostics.Debug.WriteLine("Already on Posts page, no navigation needed");
                    return;
                }

                // Existing navigation code...
            }
            catch (Exception ex)
            {
                // Existing exception handling...
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

                                // Mark that auto-login is in progress
                                UltimateHoopers.Helpers.AutoLoginNavigationHelper.IsInitialNavigationComplete = false;

                                // Give shell time to initialize
                                await Task.Delay(500);
                            }
                            catch (Exception shellEx)
                            {
                                DiagnosticHelper.LogException(shellEx, "Setting MainPage to AppShell");
                                // Fallback to creating a new AppShell
                                try
                                {
                                    MainPage = new AppShell(authService);
                                    DiagnosticHelper.Log("Set MainPage to new AppShell (fallback)");

                                    // Still mark as auto-login
                                    UltimateHoopers.Helpers.AutoLoginNavigationHelper.IsInitialNavigationComplete = false;

                                    // Give shell time to initialize
                                    await Task.Delay(500);
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
                            await Task.Delay(500);  // Increase delay for better initialization

                            // Safely set the Posts page as the current page
                            try
                            {
                                // Use the helper to ensure Shell is fully initialized
                                await UltimateHoopers.Helpers.AutoLoginNavigationHelper.EnsureShellInitializedAsync();

                                // Now try to set the initial page
                                SetInitialPage(appShell);

                                // Mark initial navigation as complete
                                UltimateHoopers.Helpers.AutoLoginNavigationHelper.IsInitialNavigationComplete = true;

                                DiagnosticHelper.Log("Initial navigation completed successfully");
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