namespace UltimateHoopers.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // TODO: Add actual authentication logic here

            // For now, we'll just validate that fields are not empty
            if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a username", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a password", "OK");
                return;
            }

            // Simulate authentication with a brief delay
            LoginButton.IsEnabled = false;
            LoginButton.Text = "Signing in...";

            await Task.Delay(1000);

            // Navigate to the home page
            await Shell.Current.GoToAsync("///HomePage");

            // Reset the login form
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            LoginButton.Text = "Sign In";
            LoginButton.IsEnabled = true;
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            // This would navigate to a registration page in a real app
            await DisplayAlert("Registration", "Registration functionality would be implemented here", "OK");
        }
    }
}