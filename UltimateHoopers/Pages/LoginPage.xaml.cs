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
                // Basic validation
                if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your username or email", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your password", "OK");
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
                else
                {
                    // Fallback login simulation
                    await Task.Delay(1000); // Simulate network delay

                    // For demo purposes, accept any non-empty credentials
                    if (!string.IsNullOrWhiteSpace(UsernameEntry.Text) && !string.IsNullOrWhiteSpace(PasswordEntry.Text))
                    {
                        var appShell = new AppShell();
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Application.Current.MainPage = appShell;
                        });
                    }
                    else
                    {
                        await DisplayAlert("Login Failed", "Please check your credentials", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                await DisplayAlert("Error", "An error occurred during login. Please try again.", "OK");
            }
            finally
            {
                // Reset button
                LoginButton.Text = "Sign In";
                LoginButton.IsEnabled = true;
            }
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Create Account button clicked");

                // Disable button to prevent multiple clicks
                CreateAccountButton.IsEnabled = false;

                // Execute on main thread to ensure UI operations work properly
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Create CreateAccountPage instance directly
                        CreateAccountPage createAccountPage;

                        if (_authService != null)
                        {
                            createAccountPage = new CreateAccountPage(_authService);
                            Debug.WriteLine("Created CreateAccountPage with auth service");
                        }
                        else
                        {
                            createAccountPage = new CreateAccountPage();
                            Debug.WriteLine("Created CreateAccountPage without auth service");
                        }

                        // Set the previous page reference for navigation back
                        createAccountPage.PreviousPage = this;

                        // Use the most direct navigation method
                        Application.Current.MainPage = createAccountPage;
                        Debug.WriteLine("Successfully set CreateAccountPage as MainPage");
                    }
                    catch (Exception innerEx)
                    {
                        Debug.WriteLine($"Error in main thread navigation: {innerEx.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to CreateAccountPage: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", $"Could not navigate to registration page: {ex.Message}", "OK");
                });
            }
            finally
            {
                // Re-enable button on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CreateAccountButton.IsEnabled = true;
                });
            }
        }
        private async void OnForgotPasswordClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Forgot Password Navigation Started ===");

                await Task.Delay(100); // Small delay to ensure UI is ready

                System.Diagnostics.Debug.WriteLine("Attempting to create ForgotPasswordPage...");

                // Test if we can create the page first
                ForgotPasswordPage forgotPasswordPage = null;

                try
                {
                    // Try the simplest constructor first
                    forgotPasswordPage = new ForgotPasswordPage();
                    System.Diagnostics.Debug.WriteLine("✓ Successfully created ForgotPasswordPage with default constructor");
                }
                catch (Exception pageCreationEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Failed to create ForgotPasswordPage: {pageCreationEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {pageCreationEx.StackTrace}");

                    await DisplayAlert("Error",
                        $"Could not create forgot password page.\n\nError: {pageCreationEx.Message}",
                        "OK");
                    return;
                }

                // Test if Application.Current is available
                if (Application.Current == null)
                {
                    await DisplayAlert("Error", "Application context is not available", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✓ Application.Current is available");

                // Set the previous page reference
                forgotPasswordPage.PreviousPage = this;

                // Navigate using the most direct method
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        System.Diagnostics.Debug.WriteLine("Setting ForgotPasswordPage as MainPage...");
                        Application.Current.MainPage = forgotPasswordPage;
                        System.Diagnostics.Debug.WriteLine("✓ Successfully set ForgotPasswordPage as MainPage");
                    });
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Failed to set MainPage: {navEx.Message}");

                    await DisplayAlert("Error",
                        $"Navigation failed.\n\nError: {navEx.Message}",
                        "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("=== Navigation Completed Successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Forgot Password Navigation Failed ===");
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await DisplayAlert("Navigation Error",
                    $"Could not open forgot password page.\n\nError: {ex.Message}\n\nPlease try again.",
                    "OK");
            }
        }
        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Go back using the helper
            await NavigationHelper.GoBack(this);
        }
    }
}