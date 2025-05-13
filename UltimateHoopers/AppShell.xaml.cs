using System.Windows.Input;

namespace UltimateHoopers
{
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
            // This allows you to navigate using Shell.Current.GoToAsync("//route")
            // Routing.RegisterRoute("statspage", typeof(Pages.StatsPage));
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
                // TODO: Implement logout functionality
                await DisplayAlert("Logout", "You have been logged out", "OK");
                // Navigate to login page
                // await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}