using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class CreateAccountPage : ContentPage
    {
        private readonly IAuthService _authService;

        // Default constructor for design time and fallback scenarios
        public CreateAccountPage()
        {
            InitializeComponent();

            // Try to get auth service from DI in case it's available
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();
        }

        // Constructor with dependency injection
        public CreateAccountPage(IAuthService authService) : this()
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(EmailEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your email address", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a username", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your full name", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a password", "OK");
                    return;
                }

                if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
                {
                    await DisplayAlert("Error", "Passwords do not match", "OK");
                    return;
                }

                if (!TermsCheckBox.IsChecked)
                {
                    await DisplayAlert("Error", "You must agree to the Terms of Service and Privacy Policy", "OK");
                    return;
                }

                // Update UI during registration
                RegisterButton.IsEnabled = false;
                RegisterButton.Text = "Creating Account...";

                // In a real app, you would call your API to register the user
                // For this demo, we'll simulate a successful registration
                await Task.Delay(2000); // Simulate network delay

                // Show success message
                await DisplayAlert("Success", "Account created successfully! You can now log in.", "OK");

                // Navigate back to login page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Registration Error", ex.Message, "OK");
            }
            finally
            {
                // Reset button
                RegisterButton.Text = "Create Account";
                RegisterButton.IsEnabled = true;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Navigate back to login page
            await Navigation.PopAsync();
        }
    }
}