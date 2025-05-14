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
        }

        private void RegisterRoutes()
        {
            // Register routes for navigation
            Routing.RegisterRoute("homepage", typeof(Pages.HomePage));
            Routing.RegisterRoute("postspage", typeof(Pages.PostsPage));
            // Add additional routes as you create more pages
        }

        private async Task ShowHelpDialog()
        {
            await DisplayAlert("Help & Support", "Help & Support page coming soon!", "OK");
        }

        private async Task PerformLogout()
        {
            bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (answer)
            {
                // Use auth service to logout if available
                if (_authService != null)
                {
                    await _authService.LogoutAsync();
                }
                else
                {
                    // Fallback if service is not available
                    App.AuthToken = null;
                    await SecureStorage.SetAsync("auth_token", string.Empty);
                    await SecureStorage.SetAsync("user_id", string.Empty);
                }

                // Try to get LoginPage from DI
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                var loginPage = serviceProvider.GetService<LoginPage>();

                // Navigate back to LoginPage
                if (loginPage != null)
                {
                    Application.Current.MainPage = loginPage;
                }
                else if (_authService != null)
                {
                    // Create LoginPage with auth service if available
                    Application.Current.MainPage = new LoginPage(_authService);
                }
                else
                {
                    // Fallback without auth service
                    Application.Current.MainPage = new LoginPage();
                }
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