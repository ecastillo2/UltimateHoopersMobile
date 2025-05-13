using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UltimateHoopers
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public ICommand HelpCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public AppShell()
        {
            InitializeComponent();

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
                // Navigate back to LoginPage
                Application.Current.MainPage = new Pages.LoginPage();
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