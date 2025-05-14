// Pages/LoginPage.xaml.cs
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using Microsoft.Extensions.DependencyInjection;

namespace UltimateHoopers.Pages
{
    public partial class LoginPage : ContentPage
    {
        private IAuthService _authService;

        // Default constructor for design time and fallback scenarios
        public LoginPage()
        {
            InitializeComponent();

            // Try to get auth service from DI in case it's available
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();
        }

        // Constructor with dependency injection
        public LoginPage(IAuthService authService) : this()
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                // Validation logic
                if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a username or email", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a password", "OK");
                    return;
                }

                // Update UI during login
                LoginButton.IsEnabled = false;
                LoginButton.Text = "Signing in...";

                if (_authService != null)
                {
                    // Use auth service to login
                    var user = await _authService.LoginAsync(UsernameEntry.Text, PasswordEntry.Text);

                    if (user != null)
                    {
                        // Get service provider to resolve AppShell
                        var serviceProvider = MauiProgram.CreateMauiApp().Services;
                        var appShell = serviceProvider.GetService<AppShell>();

                        // Set the main page to AppShell
                        if (appShell != null)
                        {
                            Application.Current.MainPage = appShell;
                        }
                        else
                        {
                            // Fallback if DI fails
                            Application.Current.MainPage = new AppShell(_authService);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Login Failed", "Invalid username or password", "OK");
                    }
                }
                else
                {
                    // Fallback for demo/testing - simulated login
                    await Task.Delay(1000); // Simulate network delay

                    // Set a dummy token
                    App.AuthToken = "sample-jwt-token-here";
                    await SecureStorage.SetAsync("auth_token", App.AuthToken);

                    // Navigate to main app
                    Application.Current.MainPage = new AppShell();

                    await DisplayAlert("Demo Mode", "Logged in with simulated credentials. Auth service not available.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Error", ex.Message, "OK");
            }
            finally
            {
                // Reset form
                LoginButton.Text = "Sign In";
                LoginButton.IsEnabled = true;
            }
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Registration", "Registration functionality would be implemented here", "OK");
        }
    }
}