using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Helpers; // Add this to import NavigationHelper
using UltimateHoopers.Services;

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
                // Existing validation logic...

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
                        await Task.Delay(300); // Increased delay for better stability

                        // Navigate to PostsPage using the helper
                        try
                        {
                            // Explicitly navigate to PostsPage
                            await Shell.Current.GoToAsync("//PostsPage");

                            // Add detailed logging
                            Debug.WriteLine("Successfully navigated to PostsPage after login");
                        }
                        catch (Exception navEx)
                        {
                            Debug.WriteLine($"Error in initial navigation to PostsPage: {navEx.Message}");

                            // Try alternative navigation
                            try
                            {
                                var postsPage = serviceProvider.GetService<PostsPage>() ?? new PostsPage();
                                await appShell.Navigation.PushAsync(postsPage);
                            }
                            catch (Exception altEx)
                            {
                                Debug.WriteLine($"Alternative navigation failed: {altEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Login Failed", "Invalid username or password", "OK");
                    }
                }

                // Existing code for fallback login...
            }
            catch (Exception ex)
            {
                // Existing exception handling...
            }
            finally
            {
                // Existing cleanup...
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