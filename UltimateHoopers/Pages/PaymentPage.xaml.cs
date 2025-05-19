using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using Domain; // Import the Domain namespace

namespace UltimateHoopers.Pages
{
    public partial class PaymentPage : ContentPage
    {
        private readonly string _email;
        private readonly string _username;
        private readonly string _fullName;
        private readonly string _password;
        private readonly IAuthService _authService;

        // Default constructor for XAML preview
        public PaymentPage()
        {
            InitializeComponent();
        }

        // Constructor with user registration data
        public PaymentPage(string email, string username, string fullName, string password, IAuthService authService)
        {
            InitializeComponent();

            _email = email;
            _username = username;
            _fullName = fullName;
            _password = password;
            _authService = authService;

            // Set the title with the username
            WelcomeLabel.Text = $"Welcome, {_fullName}!";
        }

        // Handle payment submission
        private async void OnPaymentSubmitClicked(object sender, EventArgs e)
        {
            // Validation logic
            if (string.IsNullOrWhiteSpace(CardNumberEntry.Text) ||
                string.IsNullOrWhiteSpace(ExpiryDateEntry.Text) ||
                string.IsNullOrWhiteSpace(CvvEntry.Text) ||
                string.IsNullOrWhiteSpace(CardholderNameEntry.Text))
            {
                await DisplayAlert("Error", "Please complete all payment fields", "OK");
                return;
            }

            // Show loading state
            PaymentButton.IsEnabled = false;
            PaymentButton.Text = "Processing...";

            try
            {
                // In a real application, you would:
                // 1. Send payment info to a payment processor
                // 2. Wait for confirmation
                // 3. Then create the account

                // Simulate payment processing
                await Task.Delay(2000);

                // Complete registration with Host account type
                bool registrationSuccess = false;

                if (_authService != null)
                {
                    // Register user with Host account type
                    registrationSuccess = await _authService.RegisterAsync(
                        _email,
                        _username,
                        _fullName,
                        _password,
                        AccountType.Host);
                }
                else
                {
                    // Fallback to simulated success
                    registrationSuccess = true;
                }

                if (registrationSuccess)
                {
                    await DisplayAlert("Success",
                        "Your Host account has been created and your subscription ($9.99/month) will begin today. You can now create and manage runs!",
                        "OK");

                    // Navigate back to login page
                    ReturnToLoginPage();
                }
                else
                {
                    await DisplayAlert("Registration Failed",
                        "Your payment was processed, but we couldn't create your account. Please contact support.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Payment Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                // Reset button
                PaymentButton.Text = "Complete Payment";
                PaymentButton.IsEnabled = true;
            }
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        // Helper method to return to login page
        private void ReturnToLoginPage()
        {
            try
            {
                // Navigate through page stack to return to login
                Application.Current.MainPage = new LoginPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error returning to login page: {ex.Message}");

                // Ultimate fallback
                Application.Current.MainPage = new LoginPage();
            }
        }
    }
}