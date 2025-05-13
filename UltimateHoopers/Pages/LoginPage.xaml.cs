using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace UltimateHoopers.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // IMPORTANT: Button click event uses EventArgs (NOT TappedEventArgs)
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Validation logic
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

            // Simulate login process
            LoginButton.IsEnabled = false;
            LoginButton.Text = "Signing in...";

            await Task.Delay(1000);

            // Set the main page to AppShell
            Application.Current.MainPage = new AppShell();

            // Reset form
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            LoginButton.Text = "Sign In";
            LoginButton.IsEnabled = true;
        }

        // IMPORTANT: Button click event uses EventArgs (NOT TappedEventArgs)
        // The XAML is using Clicked="OnCreateAccountClicked" for a Button
        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Registration", "Registration functionality would be implemented here", "OK");
        }
    }
}