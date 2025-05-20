using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateHoopers.Pages;
using UltimateHoopers.Services;

namespace UltimateHoopers
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        private readonly IAuthService _authService;

        public ICommand HelpCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand NavigateToNotificationsCommand { get; private set; }

        // Default constructor for XAML preview
        public AppShell()
        {
            InitializeComponent();

            // Try to get auth service from DI in case it's available
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();

            // Initialize common components
            Initialize();
        }

        // Constructor with dependency injection
        public AppShell(IAuthService authService)
        {
            InitializeComponent();

            _authService = authService;

            // Initialize common components
            Initialize();
        }

        private void Initialize()
        {
            // Register routes for navigation
            RegisterRoutes();

            // Initialize commands
            HelpCommand = new Command(async () => await ShowHelpDialog());
            LogoutCommand = new Command(async () => await PerformLogout());
            NavigateToNotificationsCommand = new Command(async () => await NavigateToNotifications());

            // Set binding context to this to use the commands
            BindingContext = this;

            // Set the Posts page as the initial page
            // Use the safer method to find and set the Posts page
            MainThread.BeginInvokeOnMainThread(async () => {
                try
                {
                    // Add a slight delay to ensure shell is fully loaded
                    await Task.Delay(100);
                    SetInitialPage(this);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in initial navigation: {ex.Message}");
                }
            });
        }

        public async Task EnsureCorrectNavigationStack()
        {
            try
            {
                Debug.WriteLine("AppShell: EnsureCorrectNavigationStack called");

                // Register all routes if not already registered
                RegisterAllRoutes();

                // Try to reset internal navigation state
                if (Navigation != null && Navigation.NavigationStack.Count > 1)
                {
                    Debug.WriteLine("AppShell: Clearing navigation stack");

                    // Clear all but the first page in the navigation stack
                    var firstPage = Navigation.NavigationStack.FirstOrDefault();
                    for (int i = Navigation.NavigationStack.Count - 1; i > 0; i--)
                    {
                        Navigation.RemovePage(Navigation.NavigationStack[i]);
                    }
                }

                Debug.WriteLine("AppShell: Navigation stack cleared/reset");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShell: Error in EnsureCorrectNavigationStack: {ex.Message}");
            }
        }

        private void RegisterRoutes()
        {
            try
            {
                // Register routes for navigation
                RegisterAllRoutes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShell: Error registering routes: {ex.Message}");

                // Try again with individual routes in try/catch blocks
                try { Routing.RegisterRoute("homepage", typeof(Pages.HomePage)); } catch { }
                try { Routing.RegisterRoute("postspage", typeof(Pages.PostsPage)); } catch { }
                try { Routing.RegisterRoute("hooperspage", typeof(Pages.HoopersPage)); } catch { }
                try { Routing.RegisterRoute("statspage", typeof(Pages.StatsPage)); } catch { }
            }
        }

        private void RegisterAllRoutes()
        {
            // Check if route is already registered before trying to register it
            if (!Routing.IsRouteRegistered("homepage"))
                Routing.RegisterRoute("homepage", typeof(Pages.HomePage));

            if (!Routing.IsRouteRegistered("postspage"))
                Routing.RegisterRoute("postspage", typeof(Pages.PostsPage));

            if (!Routing.IsRouteRegistered("hooperspage"))
                Routing.RegisterRoute("hooperspage", typeof(Pages.HoopersPage));

            if (!Routing.IsRouteRegistered("statspage"))
                Routing.RegisterRoute("statspage", typeof(Pages.StatsPage));

            if (!Routing.IsRouteRegistered("findrunspage"))
                Routing.RegisterRoute("findrunspage", typeof(Pages.FindRunsPage));

            if (!Routing.IsRouteRegistered("shoppage"))
                Routing.RegisterRoute("shoppage", typeof(Pages.ShopPage));

            if (!Routing.IsRouteRegistered("editprofilepage"))
                Routing.RegisterRoute("editprofilepage", typeof(Pages.EditProfilePage));

            if (!Routing.IsRouteRegistered("createaccount"))
                Routing.RegisterRoute("createaccount", typeof(Pages.CreateAccountPage));

            if (!Routing.IsRouteRegistered("notificationspage"))
                Routing.RegisterRoute("notificationspage", typeof(Pages.NotificationsPage));

            if (!Routing.IsRouteRegistered("notificationsettingspage"))
                Routing.RegisterRoute("notificationsettingspage", typeof(Pages.NotificationSettingsPage));
        }

        // Helper extension method to check if a route is registered
        public static class RoutingExtensions
        {
            public static bool IsRouteRegistered(this IRoutingService service, string route)
            {
                try
                {
                    var routes = Routing.GetRoutes();
                    return routes.Contains(route);
                }
                catch
                {
                    return false;
                }
            }
        }

        private async Task ShowHelpDialog()
        {
            await DisplayAlert("Help & Support", "Help & Support page coming soon!", "OK");
        }

        private async Task PerformLogout()
        {
            try
            {
                bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
                if (answer)
                {
                    // Use auth service to logout if available
                    if (_authService != null)
                    {
                        try
                        {
                            await _authService.LogoutAsync();
                        }
                        catch (Exception ex)
                        {
                            // Log the exception but continue with logout process
                            System.Diagnostics.Debug.WriteLine($"Error in auth service logout: {ex.Message}");
                            // Manual fallback for logout
                            App.AuthToken = null;
                            await SecureStorage.Default.SetAsync("auth_token", string.Empty);
                            await SecureStorage.Default.SetAsync("user_id", string.Empty);
                        }
                    }
                    else
                    {
                        // Fallback if service is not available
                        App.AuthToken = null;
                        await SecureStorage.Default.SetAsync("auth_token", string.Empty);
                        await SecureStorage.Default.SetAsync("user_id", string.Empty);
                    }

                    // Wrap the main page transition in try-catch
                    try
                    {
                        // Create a new instance of LoginPage
                        LoginPage loginPage;

                        // Try to get LoginPage from DI
                        var serviceProvider = MauiProgram.CreateMauiApp().Services;
                        var resolvedLoginPage = serviceProvider.GetService<LoginPage>();

                        if (resolvedLoginPage != null)
                        {
                            loginPage = resolvedLoginPage;
                        }
                        else if (_authService != null)
                        {
                            // Create LoginPage with auth service if available
                            loginPage = new LoginPage(_authService);
                        }
                        else
                        {
                            // Fallback without auth service
                            loginPage = new LoginPage();
                        }

                        // Important: Dispatch to main thread for UI operations
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            try
                            {
                                Application.Current.MainPage = loginPage;
                                System.Diagnostics.Debug.WriteLine("Successfully set MainPage to LoginPage");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error setting MainPage: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // Handle navigation error
                        System.Diagnostics.Debug.WriteLine($"Error navigating to login page: {ex.Message}");
                        await DisplayAlert("Error", "There was a problem logging out. Please restart the app.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled exception in logout: {ex.Message}");
                await DisplayAlert("Error", "An unexpected error occurred. Please try again.", "OK");
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

        // These methods can be used for tap gesture recognizers in Shell
        private async void OnHelpTapped(object sender, TappedEventArgs e)
        {
            await ShowHelpDialog();
        }

        private async void OnLogoutTapped(object sender, TappedEventArgs e)
        {
            await PerformLogout();
        }

        private async void OnNotificationsTapped(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Notifications", "Notifications feature coming soon!", "OK");
        }

        // Add this method to AppShell.xaml.cs
        private async Task NavigateToNotifications()
        {
            try
            {
                // Check if we're already on the notifications page
                if (Shell.Current?.CurrentPage is NotificationsPage)
                {
                    System.Diagnostics.Debug.WriteLine("Already on Notifications page, skipping navigation");
                    return;
                }

                await Shell.Current.GoToAsync("//notificationspage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to notifications page", "OK");
            }
        }
    }
}