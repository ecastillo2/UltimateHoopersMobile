// AppShell.xaml.cs
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

            // Set binding context to this to use the commands
            BindingContext = this;

            // CHANGE: Set the Posts page as the initial page
            // Use the safer method to find and set the Posts page
            SetInitialPage(this);
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
                if (shell == null || shell.Items == null || shell.Items.Count == 0)
                    return;

                // First look for a matching tab/flyout item with the exact route
                var postsItem = shell.Items.FirstOrDefault(item =>
                    item != null &&
                    "PostsPage".Equals(item.Route, StringComparison.OrdinalIgnoreCase));

                if (postsItem != null)
                {
                    shell.CurrentItem = postsItem;
                    System.Diagnostics.Debug.WriteLine("Set shell.CurrentItem to PostsPage directly");
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
                            System.Diagnostics.Debug.WriteLine("Set shell.CurrentItem to item containing PostsPage tab");
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
                        System.Diagnostics.Debug.WriteLine($"Set shell.CurrentItem to item with route containing 'Posts': {item.Route}");
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
                            System.Diagnostics.Debug.WriteLine($"Set shell.CurrentItem to item containing tab with route containing 'Posts': {tab.Route}");
                            return;
                        }
                    }
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
    }
}