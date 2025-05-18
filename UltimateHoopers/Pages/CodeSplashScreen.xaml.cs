using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Helpers;
using Microsoft.Maui.ApplicationModel;

namespace UltimateHoopers.Pages
{
    public class CodeSplashScreen : ContentPage
    {
        // UI Elements
        private Image logoImage;
        private Label titleLabel;
        private Label subtitleLabel;
        private ActivityIndicator loadingIndicator;

        public CodeSplashScreen()
        {
            try
            {
                Console.WriteLine("CodeSplashScreen constructor starting");

                // Create UI elements in code
                SetupUI();

                Console.WriteLine("CodeSplashScreen UI setup complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CodeSplashScreen constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SetupUI()
        {
            // Set background
            BackgroundColor = Colors.Purple;

            // Create layout
            var stackLayout = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 20
            };

            // Create UI elements
            logoImage = new Image
            {
                Source = "logo_uh.png",
                HeightRequest = 150,
                WidthRequest = 150,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0.1 // Start nearly invisible for animation
            };

            titleLabel = new Label
            {
                Text = "ULTIMATE HOOPERS",
                TextColor = Colors.White,
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0.1 // Start nearly invisible for animation
            };

            subtitleLabel = new Label
            {
                Text = "Pick Up Basketball League",
                TextColor = Colors.LightGray,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0.1 // Start nearly invisible for animation
            };

            loadingIndicator = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.White,
                HeightRequest = 50,
                WidthRequest = 50,
                Opacity = 0.1, // Start nearly invisible for animation
                Margin = new Thickness(0, 30, 0, 0)
            };

            // Add elements to layout
            stackLayout.Add(logoImage);
            stackLayout.Add(titleLabel);
            stackLayout.Add(subtitleLabel);
            stackLayout.Add(loadingIndicator);

            // Set content
            Content = stackLayout;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("CodeSplashScreen OnAppearing");

            try
            {
                // Animate UI elements
                await StartAnimations();

                // Simulate loading time
                await Task.Delay(1500);

                // Check authentication status
                bool isAuthenticated = await IsAuthenticated();

                // Navigate to appropriate page
                await NavigateToNextPage(isAuthenticated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CodeSplashScreen OnAppearing: {ex.Message}");

                // Fallback
                MainThread.BeginInvokeOnMainThread(() => {
                    Application.Current.MainPage = new LoginPage();
                });
            }
        }

        private async Task StartAnimations()
        {
            try
            {
                // Animate all elements in parallel
                var logoAnimation = logoImage.FadeTo(1, 800);
                var titleAnimation = titleLabel.FadeTo(1, 800);
                var subtitleAnimation = subtitleLabel.FadeTo(1, 800);
                var loadingAnimation = loadingIndicator.FadeTo(1, 800);

                // Wait for all animations to complete
                await Task.WhenAll(logoAnimation, titleAnimation, subtitleAnimation, loadingAnimation);

                // Add bounce effect to logo
                await logoImage.ScaleTo(1.1, 200);
                await logoImage.ScaleTo(1.0, 200);

                Console.WriteLine("Animations completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Animation error: {ex.Message}");
                // Continue even if animations fail
            }
        }

        private async Task<bool> IsAuthenticated()
        {
            try
            {
                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;

                // Get auth service
                var authService = serviceProvider.GetService<Services.IAuthService>();

                if (authService != null)
                {
                    // Check if user is authenticated
                    return await authService.IsAuthenticatedAsync();
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication check error: {ex.Message}");
                return false;
            }
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
                    "PostsPage".Equals(item.Route, StringComparison.OrdinalIgnoreCase));

                if (postsItem != null)
                {
                    shell.CurrentItem = postsItem;
                    Console.WriteLine("Set shell.CurrentItem to PostsPage directly");
                    return;
                }

                // Then look for an item containing a tab with the route
                foreach (var item in shell.Items)
                {
                    if (item?.Items != null)
                    {
                        var tab = item.Items.FirstOrDefault(si =>
                            si != null &&
                            "PostsPage".Equals(si.Route, StringComparison.OrdinalIgnoreCase));

                        if (tab != null)
                        {
                            shell.CurrentItem = item;
                            if (item.CurrentItem != tab)
                            {
                                item.CurrentItem = tab;
                            }
                            Console.WriteLine("Set shell.CurrentItem to item containing PostsPage tab");
                            return;
                        }
                    }
                }

                // If we didn't find a Posts page specifically, try to find any page that has "Posts" in its name
                foreach (var item in shell.Items)
                {
                    if (item?.Route != null && item.Route.Contains("Posts", StringComparison.OrdinalIgnoreCase))
                    {
                        shell.CurrentItem = item;
                        Console.WriteLine($"Set shell.CurrentItem to item with route containing 'Posts': {item.Route}");
                        return;
                    }

                    if (item?.Items != null)
                    {
                        var tab = item.Items.FirstOrDefault(si =>
                            si?.Route != null &&
                            si.Route.Contains("Posts", StringComparison.OrdinalIgnoreCase));

                        if (tab != null)
                        {
                            shell.CurrentItem = item;
                            if (item.CurrentItem != tab)
                            {
                                item.CurrentItem = tab;
                            }
                            Console.WriteLine($"Set shell.CurrentItem to item containing tab with route containing 'Posts': {tab.Route}");
                            return;
                        }
                    }
                }

                Console.WriteLine("Could not find Posts page in shell items");
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app if setting initial page fails
                Console.WriteLine($"Error setting initial page to Posts: {ex.Message}");
            }
        }// Update the NavigateToNextPage method in CodeSplashScreen.xaml.cs
        private async Task NavigateToNextPage(bool isAuthenticated)
        {
            try
            {
                // Fade out the splash screen
                await this.FadeTo(0, 300);

                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;

                // Navigate to the appropriate page
                await MainThread.InvokeOnMainThreadAsync(() => {
                    if (isAuthenticated)
                    {
                        // Get AppShell
                        var appShell = serviceProvider.GetService<AppShell>();
                        if (appShell != null)
                        {
                            // Set the shell as the main page
                            Application.Current.MainPage = appShell;

                            // Safely navigate to Posts page
                            SetInitialPage(appShell);
                        }
                        else
                        {
                            // Fallback
                            var authService = serviceProvider.GetService<Services.IAuthService>();
                            var shell = new AppShell(authService);
                            Application.Current.MainPage = shell;

                            // Safely navigate to Posts page
                            SetInitialPage(shell);
                        }
                    }
                    else
                    {
                        // Get LoginPage
                        var loginPage = serviceProvider.GetService<LoginPage>();
                        if (loginPage != null)
                        {
                            Application.Current.MainPage = loginPage;
                        }
                        else
                        {
                            // Fallback
                            Application.Current.MainPage = new LoginPage();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");

                // Fallback
                await MainThread.InvokeOnMainThreadAsync(() => {
                    Application.Current.MainPage = new LoginPage();
                });
            }
        }
    }
}