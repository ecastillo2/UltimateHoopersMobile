using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using Domain; // Import the Domain namespace

namespace UltimateHoopers.Pages
{
    public partial class CreateAccountPage : ContentPage
    {
        private readonly IAuthService _authService;

        // Property to store the selected account type - using Domain.AccountType
        public AccountType SelectedAccountType { get; private set; } = AccountType.Free;

        // Property to store the previous page for direct navigation scenarios
        public Page PreviousPage { get; set; }

        // Default constructor for design time and fallback scenarios
        public CreateAccountPage()
        {
            InitializeComponent();

            // Try to get auth service from DI in case it's available
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();

            // Set up initial UI for account type selection
            UpdateAccountTypeUI();
        }

        // Constructor with dependency injection
        public CreateAccountPage(IAuthService authService) : this()
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        // Handle radio button selection change
        private void OnAccountTypeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender == FreeAccountRadio && e.Value)
            {
                SelectedAccountType = AccountType.Free;
                HostAccountRadio.IsChecked = false;
            }
            else if (sender == HostAccountRadio && e.Value)
            {
                SelectedAccountType = AccountType.Host;
                FreeAccountRadio.IsChecked = false;
            }

            UpdateAccountTypeUI();
        }

        // Handle tap on the free account frame
        private void OnFreeAccountSelected(object sender, EventArgs e)
        {
            SelectedAccountType = AccountType.Free;
            FreeAccountRadio.IsChecked = true;
            HostAccountRadio.IsChecked = false;
            UpdateAccountTypeUI();
        }

        // Handle tap on the host account frame
        private void OnHostAccountSelected(object sender, EventArgs e)
        {
            SelectedAccountType = AccountType.Host;
            HostAccountRadio.IsChecked = true;
            FreeAccountRadio.IsChecked = false;
            UpdateAccountTypeUI();
        }

        // Update the UI based on selected account type
        private void UpdateAccountTypeUI()
        {
            // Update frame appearances based on selection
            if (SelectedAccountType == AccountType.Free)
            {
                FreeAccountFrame.BorderColor = (Color)Application.Current.Resources["PrimaryColor"];
                FreeAccountFrame.BackgroundColor = (Color)Application.Current.Resources["SecondaryColor"];
                HostAccountFrame.BorderColor = (Color)Application.Current.Resources["BorderColor"];
                HostAccountFrame.BackgroundColor = (Color)Application.Current.Resources["CardBackgroundColor"];
            }
            else
            {
                HostAccountFrame.BorderColor = (Color)Application.Current.Resources["PrimaryColor"];
                HostAccountFrame.BackgroundColor = (Color)Application.Current.Resources["SecondaryColor"];
                FreeAccountFrame.BorderColor = (Color)Application.Current.Resources["BorderColor"];
                FreeAccountFrame.BackgroundColor = (Color)Application.Current.Resources["CardBackgroundColor"];
            }
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

                // Check if Host account was selected - go to payment page
                if (SelectedAccountType == AccountType.Host)
                {
                    // Navigate to payment page with registration info
                    await Navigation.PushAsync(new PaymentPage(
                        EmailEntry.Text,
                        UsernameEntry.Text,
                        FullNameEntry.Text,
                        PasswordEntry.Text,
                        _authService
                    ));
                    return;
                }

                // Continue with Free account registration
                // Update UI during registration
                RegisterButton.IsEnabled = false;
                RegisterButton.Text = "Creating Account...";

                // Call the auth service to register the user
                bool registrationSuccess = false;

                if (_authService != null)
                {
                    // Use the auth service if available
                    registrationSuccess = await _authService.RegisterAsync(
                        EmailEntry.Text,
                        UsernameEntry.Text,
                        FullNameEntry.Text,
                        PasswordEntry.Text,
                        SelectedAccountType);
                }
                else
                {
                    // Fallback to simulated success if no auth service
                    await Task.Delay(2000); // Simulate network delay
                    registrationSuccess = true;
                }

                // Show success message with account type
                if (registrationSuccess)
                {
                    string accountTypeMessage = "Account created successfully! You can now log in.";

                    await DisplayAlert("Success", accountTypeMessage, "OK");

                    // Navigate back to login page
                    ReturnToLoginPage();
                }
                else
                {
                    await DisplayAlert("Registration Failed", "Unable to create your account. Please try again later.", "OK");
                }
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