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

        public Command<string> CreateAccountCommand => new Command<string>(async (route) => {
            try
            {
                await Shell.Current.GoToAsync(route);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open registration page: {ex.Message}", "OK");
            }
        });
        // Constructor with dependency injection
        public LoginPage(IAuthService authService) : this()
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        private void SetInitialPage(Shell shell)
        {
            try
            {
                if (shell == null || shell.Items == null || shell.Items.Count == 0)
                    return;

                // First look for a matching tab/flyout item with the exact route
                var postsItem = shell.Items.FirstOrDefault(item =>
                    item != null &&
                    item.Route != null &&
                    item.Route.Equals("PostsPage", StringComparison.OrdinalIgnoreCase));

                if (postsItem != null)
                {
                    shell.CurrentItem = postsItem;
                    System.Diagnostics.Debug.WriteLine("Set shell.CurrentItem to PostsPage directly");
                    return;
                }

                // Then look for ShellContent with the route
                foreach (var item in shell.Items)
                {
                    if (item?.Items != null)
                    {
                        var shellContent = item.Items.FirstOrDefault(si =>
                            si != null &&
                            si.Route != null &&
                            si.Route.Equals("PostsPage", StringComparison.OrdinalIgnoreCase));

                        if (shellContent != null)
                        {
                            shell.CurrentItem = item;
                            if (item.CurrentItem != shellContent)
                            {
                                item.CurrentItem = shellContent;
                            }
                            System.Diagnostics.Debug.WriteLine("Set shell.CurrentItem to item containing PostsPage tab");
                            return;
                        }
                    }
                }

                // If explicit route not found, try GoToAsync
                try
                {
                    // This is an alternative approach that uses shell navigation directly
                    Shell.Current.GoToAsync("//PostsPage");
                    System.Diagnostics.Debug.WriteLine("Used Shell.GoToAsync to navigate to PostsPage");
                    return;
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"GoToAsync navigation error: {navEx.Message}");
                }

                // Log that we couldn't find the page
                System.Diagnostics.Debug.WriteLine("Could not find PostsPage in shell items");
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app if setting initial page fails
                System.Diagnostics.Debug.WriteLine($"Error setting initial page to Posts: {ex.Message}");
            }
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

                        // Try to set the main page to AppShell and navigate to PostsPage
                        try
                        {
                            var appShell = serviceProvider.GetService<AppShell>();

                            if (appShell != null)
                            {
                                // Set the AppShell as the main page
                                Application.Current.MainPage = appShell;

                                // Ensure we give the UI time to process the MainPage change before navigation
                                await Task.Delay(100);

                                // Try to navigate using Shell.GoToAsync
                                await Shell.Current.GoToAsync("//PostsPage");

                                // Also try using SetInitialPage as a fallback
                                SetInitialPage(appShell);
                            }
                            else
                            {
                                // Fallback if DI fails
                                var shell = new AppShell(_authService);
                                Application.Current.MainPage = shell;

                                await Task.Delay(100);

                                await Shell.Current.GoToAsync("//PostsPage");

                                // Safely set the Posts page as the current page
                                SetInitialPage(shell);
                            }
                        }
                        catch (Exception navEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Navigation error: {navEx.Message}");

                            // Last resort fallback - create PostsPage directly
                            Application.Current.MainPage = new NavigationPage(new PostsPage());
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
                        // Create a new shell and set it as main page
                        Application.Current.MainPage = new AppShell();

                        // Ensure we give the UI time to process the MainPage change
                        await Task.Delay(100);

                        // Try to navigate to PostsPage
                        await Shell.Current.GoToAsync("//PostsPage");
                    }
                    catch (Exception navEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Navigation error in fallback: {navEx.Message}");

                        // Direct fallback
                        Application.Current.MainPage = new NavigationPage(new PostsPage());
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

                // Create the page directly
                var createAccountPage = new CreateAccountPage();

                // Store the current LoginPage as the previous page
                createAccountPage.PreviousPage = this;

                // Wrap in NavigationPage to enable back button
                var navPage = new NavigationPage(createAccountPage);

                // Set as MainPage directly
                Application.Current.MainPage = navPage;

                Console.WriteLine("Directly set CreateAccountPage as MainPage wrapped in NavigationPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Direct navigation error: {ex.Message}");
                DisplayAlert("Error", "Could not navigate to registration page", "OK");
            }
            finally
            {
                CreateAccountButton.IsEnabled = true;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Use Shell navigation instead of Navigation.PopAsync
            await Shell.Current.GoToAsync("..");
        }
    }
}