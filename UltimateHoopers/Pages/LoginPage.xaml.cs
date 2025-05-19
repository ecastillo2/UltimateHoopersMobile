using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using Microsoft.Extensions.DependencyInjection;
using UltimateHoopers.Helpers; // Add this to import NavigationHelper

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
                        // Success! Navigate to the main application shell
                        var serviceProvider = MauiProgram.CreateMauiApp().Services;
                        var appShell = serviceProvider.GetService<AppShell>() ?? new AppShell(_authService);

                        // Important: Dispatch to main thread for UI operations
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Application.Current.MainPage = appShell;
                        });

                        // Wait a moment for the shell to initialize
                        await Task.Delay(100);

                        // Navigate to PostsPage using the helper
                        try
                        {
                            // Navigate using our helper
                            await NavigationHelper.NavigateTo(appShell, "//PostsPage");
                        }
                        catch (Exception navEx)
                        {
                            // Log but continue - the app shell will try to navigate to PostsPage as well
                            System.Diagnostics.Debug.WriteLine($"Initial navigation error: {navEx.Message}");
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

                    try
                    {
                        // Set app to use Shell navigation
                        NavigationHelper.SetRootToShell();

                        // Wait a moment for the shell to initialize
                        await Task.Delay(100);

                        // Navigate to PostsPage using the helper
                        await NavigationHelper.NavigateTo(null, "//PostsPage");
                    }
                    catch (Exception navEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Navigation error in fallback: {navEx.Message}");
                    }

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

        private void OnCreateAccountClicked(object sender, EventArgs e)
        {
            try
            {
                // Disable button to prevent multiple clicks
                CreateAccountButton.IsEnabled = false;

                // Navigate using the helper
                NavigationHelper.NavigateTo(this, "//CreateAccountPage").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Direct navigation error: {ex.Message}");
                DisplayAlert("Error", "Could not navigate to registration page", "OK").ConfigureAwait(false);
            }
            finally
            {
                CreateAccountButton.IsEnabled = true;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Go back using the helper
            await NavigationHelper.GoBack(this);
        }
    }
}