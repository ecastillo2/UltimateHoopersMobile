using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
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

        private void RegisterRoutes()
        {
            // Register routes for navigation
            Routing.RegisterRoute("homepage", typeof(Pages.HomePage));
            Routing.RegisterRoute("postspage", typeof(Pages.PostsPage));
            Routing.RegisterRoute("hooperspage", typeof(Pages.HoopersPage));
            Routing.RegisterRoute("statspage", typeof(Pages.StatsPage));
            Routing.RegisterRoute("findrunspage", typeof(Pages.FindRunsPage));
            Routing.RegisterRoute("shoppage", typeof(Pages.ShopPage));
            Routing.RegisterRoute("editprofilepage", typeof(Pages.EditProfilePage));
            Routing.RegisterRoute("createaccount", typeof(CreateAccountPage));
            Routing.RegisterRoute("notificationspage", typeof(Pages.NotificationsPage));
            Routing.RegisterRoute("notificationsettingspage", typeof(Pages.NotificationSettingsPage));
            // Add additional routes as you create more pages
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
                            Application.Current.MainPage = loginPage;
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
                    Shell.Current.GoToAsync("//PostsPage");
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