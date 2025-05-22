using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltimateHoopers.Services;
// Create alias for the Animation classes to avoid ambiguity
using ControlsAnimation = Microsoft.Maui.Controls.Animation;
using MauiAnimation = Microsoft.Maui.Animations.Animation;

namespace UltimateHoopers.Pages
{
    public partial class HomePage : ContentPage
    {
        private readonly IAuthService _authService;
        private bool isMenuOpen = false;

        public HomePage()
        {
            InitializeComponent();
            SetupMenu();

            // Try to get auth service from DI in case it's available
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();

            // You can load user data here if needed
            LoadUserData();
        }

        // Set up side menu items
        private void SetupMenu()
        {
            // Define menu items
            var menuItems = new List<(string Icon, string Title, Action Callback)>
            {
              
                ("👤", "Account", () => Navigation.PushAsync(new AccountSettingsPage())),
                ("❓", "FAQ", () => Navigation.PushAsync(new FAQPage())),
                ("ℹ️", "About App", () => Navigation.PushAsync(new AboutPage())),
                ("🔓", "Logout", () => LogoutUser())
            };

            // Add menu items to container
            foreach (var item in menuItems)
            {
                var menuItem = CreateMenuItem(item.Icon, item.Title, item.Callback);
                MenuItemsContainer.Children.Add(menuItem);
            }
        }
        private async void OnNotificationsClicked(object sender, EventArgs e)
        {
            // You would typically load notifications from your database or API
            // For now, we'll just show a placeholder list
            await DisplayNotificationsPanel();
        }

        private async Task DisplayNotificationsPanel()
        {
            // Sample notifications - in a real app, you would fetch these from your backend
            var notifications = new List<(string Message, string Time, bool IsRead)>
    {
        ("John Smith liked your post", "1h ago", false),
        ("Sarah invited you to a game tomorrow", "3h ago", false),
        ("Basketball tournament registration closing soon", "5h ago", false),
        ("Michael commented on your highlight", "Yesterday", true),
        ("New court added near your location", "2d ago", true),
    };

            // Create notifications panel layout
            var notificationLayout = new VerticalStackLayout
            {
                Spacing = 15,
                Padding = new Thickness(20),
                BackgroundColor = Colors.White
            };

            // Add header
            notificationLayout.Add(new Label
            {
                Text = "Notifications",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current.Resources["PrimaryTextColor"],
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Add each notification
            foreach (var (message, time, isRead) in notifications)
            {
                var frame = new Frame
                {
                    Padding = new Thickness(15),
                    CornerRadius = 10,
                    BackgroundColor = isRead ? Colors.White : (Color)Application.Current.Resources["SecondaryColor"],
                    BorderColor = (Color)Application.Current.Resources["BorderColor"],
                    HasShadow = true
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
                };

                var contentStack = new VerticalStackLayout { Spacing = 5 };
                contentStack.Add(new Label
                {
                    Text = message,
                    FontSize = 16,
                    TextColor = (Color)Application.Current.Resources["PrimaryTextColor"]
                });

                contentStack.Add(new Label
                {
                    Text = time,
                    FontSize = 12,
                    TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
                });

                grid.Add(contentStack);
                Grid.SetColumn(contentStack, 0);

                if (!isRead)
                {
                    var indicator = new Ellipse
                    {
                        Fill = new SolidColorBrush((Color)Application.Current.Resources["PrimaryColor"]),
                        WidthRequest = 10,
                        HeightRequest = 10,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    grid.Add(indicator);
                    Grid.SetColumn(indicator, 1);
                }

                frame.Content = grid;
                notificationLayout.Add(frame);
            }

            // Mark all as read button
            var markAllReadButton = new Button
            {
                Text = "Mark All as Read",
                BackgroundColor = Colors.Transparent,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"],
                BorderColor = (Color)Application.Current.Resources["PrimaryColor"],
                BorderWidth = 1,
                CornerRadius = 5,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 10, 0, 0)
            };

            notificationLayout.Add(markAllReadButton);

            // Show in a popup
            var scrollView = new ScrollView { Content = notificationLayout };
            var closeButton = new Button
            {
                Text = "Close",
                BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"],
                TextColor = Colors.White,
                CornerRadius = 25,
                HeightRequest = 50,
                Margin = new Thickness(20)
            };

            var popup = new ContentPage
            {
                Content = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            },
                    Children =
            {
                scrollView,
                closeButton
            }
                }
            };

            // Set grid positioning for children
            Grid.SetRow(scrollView, 0);
            Grid.SetRow(closeButton, 1);

            closeButton.Clicked += (s, e) =>
            {
                // Update notification badge count - in real app this would be based on unread count
                UpdateNotificationBadge(0);
                Navigation.PopModalAsync();
            };

            markAllReadButton.Clicked += (s, e) =>
            {
                // In a real app, you would call an API to mark notifications as read
                // Then update UI accordingly
                UpdateNotificationBadge(0);
            };

            await Navigation.PushModalAsync(popup);
        }

        // Call this method to update the notification badge count
        public void UpdateNotificationBadge(int count)
        {
            if (count > 0)
            {
                NotificationBadge.IsVisible = true;
                NotificationCountLabel.Text = count > 9 ? "9+" : count.ToString();
            }
            else
            {
                NotificationBadge.IsVisible = false;
            }
        }

        private void InitializeNotifications()
        {
            // For demo purposes, show 3 notifications
            // In a real app, you would get this count from your backend
            UpdateNotificationBadge(3);
        }
        // Create individual menu item
        private Frame CreateMenuItem(string icon, string title, Action callback)
        {
            var menuItem = new Frame
            {
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(20, 15),
                HasShadow = false,
                BorderColor = Colors.Transparent
            };

            var layout = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(30) },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            var iconLabel = new Label
            {
                Text = icon,
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center
            };

            var titleLabel = new Label
            {
                Text = title,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#333333")
            };

            layout.Add(iconLabel, 0, 0);
            layout.Add(titleLabel, 1, 0);
            menuItem.Content = layout;

            if (callback != null)
            {
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    // Close menu
                    CloseMenu();
                    // Execute callback
                    callback.Invoke();
                };
                menuItem.GestureRecognizers.Add(tapGesture);
            }

            return menuItem;
        }

        // Load user profile data
        private void LoadUserData()
        {
            // You might want to load this from a service or local storage
            // For now, using placeholder data
            UsernameLabel.Text = App.User.Profile.UserName;
            

            // Load profile image if available
            if (!string.IsNullOrEmpty(App.User.Profile.ImageURL))
            {
                try
                {
                    ProfileImage.Source = App.User.Profile.ImageURL;
                    ProfileImage.IsVisible = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading profile image: {ex.Message}");
                }
            }
        }

        #region Menu Functions
        // IMPORTANT: Fixed event handler signature for XAML compatibility
        private void OnHamburgerButtonTapped(object sender, TappedEventArgs e)
        {
            if (isMenuOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        // This is the critical method for closing when tapping outside
        private void OnMenuOverlayTapped(object sender, TappedEventArgs e)
        {
            // Log for debugging
            Console.WriteLine("Menu overlay tapped - closing menu");
            CloseMenu();
        }

        private void OpenMenu()
        {
            if (isMenuOpen)
                return;

            // Make menu flyout visible first
            MenuFlyout.IsVisible = true;

            // The key is to ensure the overlay is NOT input transparent when menu is open
            // so it can receive taps to close the menu
            MenuOverlay.InputTransparent = false;

            // Animate menu panel
            var menuAnimation = new ControlsAnimation(v => MenuPanel.TranslationX = -280 + (v * 280), 0, 1);
            menuAnimation.Commit(this, "OpenMenu", 16, 250, Easing.CubicOut);

            // Animate overlay
            var overlayAnimation = new ControlsAnimation(v => MenuOverlay.Opacity = v * 0.5, 0, 1);
            overlayAnimation.Commit(this, "OverlayFade", 16, 250, Easing.CubicOut);

            isMenuOpen = true;
        }

        private void CloseMenu()
        {
            if (!isMenuOpen)
                return;

            // Animate menu panel
            var menuAnimation = new ControlsAnimation(v => MenuPanel.TranslationX = v * -280, 0, 1);
            menuAnimation.Commit(this, "CloseMenu", 16, 250, Easing.CubicIn, (v, c) =>
            {
                // Only hide the flyout when animation completes
                MenuFlyout.IsVisible = false;
            });

            // Animate overlay
            var overlayAnimation = new ControlsAnimation(v => MenuOverlay.Opacity = 0.5 - (v * 0.5), 0, 1);
            overlayAnimation.Commit(this, "OverlayFadeOut", 16, 250, Easing.CubicIn);

            // Set overlay to input transparent
            MenuOverlay.InputTransparent = true;
            isMenuOpen = false;
        }
        #endregion

        #region Navigation Functions

        // Add this method to your HomePage.xaml.cs file in the #region Navigation Functions section

        private async void OnViewRunDetailsClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is string runId)
                {
                    // You can use the runId here to navigate to specific run details
                    await Navigation.PushAsync(new FindRunsPage());
                }
                else
                {
                    await Navigation.PushAsync(new FindRunsPage());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to run details: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to run details", "OK");
            }
        }

        private async void OnViewAllRunsClicked(object sender, EventArgs e)
{
    try
    {
        await Navigation.PushAsync(new FindRunsPage());
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error navigating to FindRunsPage: {ex.Message}");
        await DisplayAlert("Navigation Error", "Could not navigate to runs page", "OK");
    }
}
        // IMPORTANT: Fixed event handler signatures for XAML compatibility
        private async void OnProfileClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new EditProfilePage());
        }

        private async void OnStatsClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new StatsPage());
        }

        private async void OnFindGamesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FindRunsPage());
        }

        private async void OnHoopersClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new HoopersPage());
        }

        private async void OnTeamsClicked(object sender, TappedEventArgs e)
        {
            //await Navigation.PushAsync(new TeamsPage());
            await DisplayAlert("Squads", "Feature coming soon?", "Ok");
        }

        private async void OnPostsClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new PostsPage());
        }

        private async void OnShopClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new ShopPage());
        }

        private async void OnLeaguesClicked(object sender, TappedEventArgs e)
        {
            //await Navigation.PushAsync(new LeaguesPage());
            await DisplayAlert("Leagues", "Feature coming soon?", "Ok");
        }

        private async void OnEventsClicked(object sender, TappedEventArgs e)
        {
            //await Navigation.PushAsync(new EventsPage());
            await DisplayAlert("Events", "Feature coming soon?", "Ok");
        }

        private async void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new PostsPage());
        }

        private async void OnMessagesNavigationClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new MessagesPage());
        }

        private async void OnSettingsNavigationClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
        #endregion

        // Handle back button presses (Android)
        protected override bool OnBackButtonPressed()
        {
            // If menu is open, close it and consume the back button press
            if (isMenuOpen)
            {
                CloseMenu();
                return true;
            }

            // Otherwise, let the back button work normally
            return base.OnBackButtonPressed();
        }

        // Handle logout
        private async void LogoutUser()
        {
            try
            {
                bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
                if (answer)
                {
                    // Use auth service to logout if available
                    if (_authService != null)
                    {
                        try
                        {
                            await _authService.LogoutAsync();
                        }
                        catch (Exception ex)
                        {
                            // Log the exception but continue with logout process
                            System.Diagnostics.Debug.WriteLine($"Error in auth service logout: {ex.Message}");
                            // Manual fallback for logout
                            App.AuthToken = null;
                            await SecureStorage.Default.SetAsync("auth_token", string.Empty);
                            await SecureStorage.Default.SetAsync("user_id", string.Empty);
                        }
                    }
                    else
                    {
                        // Fallback if service is not available
                        App.AuthToken = null;
                        await SecureStorage.Default.SetAsync("auth_token", string.Empty);
                        await SecureStorage.Default.SetAsync("user_id", string.Empty);
                    }

                    // Wrap the main page transition in try-catch
                    try
                    {
                        // Create a new instance of LoginPage
                        LoginPage loginPage;

                        // Try to get LoginPage from DI
                        var serviceProvider = MauiProgram.CreateMauiApp().Services;
                        var resolvedLoginPage = serviceProvider.GetService<LoginPage>();

                        if (resolvedLoginPage != null)
                        {
                            loginPage = resolvedLoginPage;
                        }
                        else if (_authService != null)
                        {
                            // Create LoginPage with auth service if available
                            loginPage = new LoginPage(_authService);
                        }
                        else
                        {
                            // Fallback without auth service
                            loginPage = new LoginPage();
                        }

                        // Important: Dispatch to main thread for UI operations
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            try
                            {
                                Application.Current.MainPage = loginPage;
                                System.Diagnostics.Debug.WriteLine("Successfully set MainPage to LoginPage");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error setting MainPage: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // Handle navigation error
                        System.Diagnostics.Debug.WriteLine($"Error navigating to login page: {ex.Message}");
                        await DisplayAlert("Error", "There was a problem logging out. Please restart the app.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled exception in logout: {ex.Message}");
                await DisplayAlert("Error", "An unexpected error occurred. Please try again.", "OK");
            }
        }

        // Animation helpers
        private async Task AnimateCardPress(Frame frame)
        {
            await frame.ScaleTo(0.95, 100, Easing.CubicOut);
            await frame.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

    // Placeholder pages for navigation
    public class ProfilePage : ContentPage { }
    //public class StatsPage : ContentPage { }
    public class FindGamesPage : ContentPage { }
    //public class HoopersPage : ContentPage { }
    public class TeamsPage : ContentPage { }
    //public class PostsPage : ContentPage { }
   // public class ShopPage : ContentPage { }
    public class LeaguesPage : ContentPage { }
    public class EventsPage : ContentPage { }
    public class SettingsPage : ContentPage { }
    public class HelpPage : ContentPage { }
    
    public class MessagesPage : ContentPage { }
}