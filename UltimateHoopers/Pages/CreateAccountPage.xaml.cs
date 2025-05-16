using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class CreateAccountPage : ContentPage
    {
        private readonly IAuthService _authService;

        // Property to store the previous page for direct navigation scenarios
        public Page PreviousPage { get; set; }

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

                //if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
                //{
                //    await DisplayAlert("Error", "Please enter your full name", "OK");
                //    return;
                //}

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
                ReturnToLoginPage();
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

        private void OnBackClicked(object sender, EventArgs e)
        {
            ReturnToLoginPage();
        }

        // Helper method to return to login page
        private void ReturnToLoginPage()
        {
            try
            {
                // Check for navigation stack first
                if (Navigation != null && Navigation.NavigationStack.Count > 1)
                {
                    Navigation.PopAsync();
                    return;
                }

                // If we have a stored previous page, use it
                if (PreviousPage != null)
                {
                    Application.Current.MainPage = PreviousPage;
                    return;
                }

                // Last resort: create a new login page
                Application.Current.MainPage = new LoginPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning to login page: {ex.Message}");

                // Ultimate fallback
                Application.Current.MainPage = new LoginPage();
            }
        }

        // Handle hardware back button
        protected override bool OnBackButtonPressed()
        {
            ReturnToLoginPage();
            return true; // Indicate we've handled it
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Print diagnostic information
            Console.WriteLine($"CreateAccountPage.OnAppearing - MainPage type: {Application.Current?.MainPage?.GetType().Name ?? "null"}");
        }
    }
}