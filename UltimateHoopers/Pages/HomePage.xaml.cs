using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Diagnostics;
using UltimateHoopers.Helpers;
using UltimateHoopers.Pages;
using UltimateHoopers.Controls;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        // Add a field for the menu popup
        private Frame _menuPopup;

        public HomePage()
        {
            InitializeComponent();
            InitializeUserProfile();

            // Set up the hamburger menu
            SetupHamburgerMenu();
        }

        private void SetupHamburgerMenu()
        {
            try
            {
                // Create the menu popup that will be shown when hamburger menu is clicked
                _menuPopup = new Frame
                {
                    IsVisible = false,
                    BackgroundColor = Colors.Transparent,
                    Padding = 0,
                    HasShadow = false,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                    ZIndex = 999, // Ensure it appears above other content
                    Margin = new Thickness(10, 80, 0, 0) // Position it below the header
                };

                var grid = new Grid();

                // Semi-transparent background
                var overlay = new BoxView
                {
                    Color = Colors.Black,
                    Opacity = 0.5
                };

                var overlayTapGesture = new TapGestureRecognizer();
                overlayTapGesture.Tapped += OnOverlayTapped;
                overlay.GestureRecognizers.Add(overlayTapGesture);

                grid.Add(overlay);

                // Menu panel
                var menuPanel = new Frame
                {
                    HeightRequest = 400,
                    WidthRequest = 250,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 0, 0, 0),
                    BorderColor = Colors.LightGray,
                    BackgroundColor = Colors.White,
                    CornerRadius = 10,
                    Padding = 0,
                    HasShadow = true
                };

                var menuStack = new VerticalStackLayout { Spacing = 0 };

                // Menu Header
                var headerGrid = new Grid
                {
                    BackgroundColor = Colors.Purple,
                    HeightRequest = 60,
                    Padding = new Thickness(15)
                };

                headerGrid.Add(new Label
                {
                    Text = "Menu",
                    TextColor = Colors.White,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center
                });

                menuStack.Add(headerGrid);

                // Menu Items Stack
                var menuItemsStack = new VerticalStackLayout { Padding = 0, Spacing = 0 };

                // Add menu items with handlers
                menuItemsStack.Add(CreateMenuItem("👤", "My Profile", ProfileItemTapped));
                menuItemsStack.Add(CreateMenuItem("⚙️", "Settings", SettingsItemTapped));
                menuItemsStack.Add(CreateMenuItem("🔔", "Notifications", NotificationsItemTapped));
                menuItemsStack.Add(CreateMenuItem("❓", "Help & Support", HelpItemTapped));
                menuItemsStack.Add(CreateMenuItem("🚪", "Logout", LogoutItemTapped));

                menuStack.Add(menuItemsStack);
                menuPanel.Content = menuStack;
                grid.Add(menuPanel);

                _menuPopup.Content = grid;

                // Add the menu popup to the page content
                (Content as Grid)?.Children.Add(_menuPopup);

                // Add hamburger menu button to the header
                var hamburgerButton = new Frame
                {
                    CornerRadius = 25,
                    HeightRequest = 50,
                    WidthRequest = 50,
                    Padding = 0,
                    BorderColor = Colors.LightGray,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };

                var hamburgerLabel = new Label
                {
                    Text = "☰",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                hamburgerButton.Content = hamburgerLabel;

                // Add tap gesture to hamburger button
                var hamburgerTapGesture = new TapGestureRecognizer();
                hamburgerTapGesture.Tapped += OnHamburgerButtonTapped;
                hamburgerButton.GestureRecognizers.Add(hamburgerTapGesture);

                // Find the header grid and add the hamburger button
                var headerGrid1 = (Content as Grid)?.Children.FirstOrDefault(c => c is Grid) as Grid;
                if (headerGrid1 != null)
                {
                    // Add hamburger button to the start of the grid
                    var logoImage = headerGrid1.Children.FirstOrDefault(c => c is Image) as Image;
                    if (logoImage != null)
                    {
                        // Adjust the logo position
                        Grid.SetColumn(logoImage, 1);
                    }

                    // Add the hamburger button
                    headerGrid1.Children.Add(hamburgerButton);
                    Grid.SetColumn(hamburgerButton, 0);
                }
                else
                {
                    Debug.WriteLine("HomePage: Could not find header grid");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error setting up hamburger menu: {ex.Message}");
            }
        }

        private Frame CreateMenuItem(string icon, string title, EventHandler<TappedEventArgs> tappedHandler)
        {
            var menuItem = new Frame
            {
                BackgroundColor = Colors.Transparent,
                BorderColor = Colors.Transparent,
                Padding = new Thickness(15),
                HeightRequest = 60
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            grid.Add(new Label
            {
                Text = icon,
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 15, 0)
            }, 0, 0);

            grid.Add(new Label
            {
                Text = title,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.DarkGray
            }, 1, 0);

            menuItem.Content = grid;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += tappedHandler;
            menuItem.GestureRecognizers.Add(tapGesture);

            return menuItem;
        }

        // Menu items tap handlers
        private void OnOverlayTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = false;
        }

        private async void ProfileItemTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = false;
            await NavigateToPage("EditProfilePage");
        }

        private async void SettingsItemTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = false;
            await DisplayAlert("Settings", "Settings page coming soon!", "OK");
        }

        private async void NotificationsItemTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = false;
            await DisplayAlert("Notifications", "Notifications page coming soon!", "OK");
        }

        private async void HelpItemTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = false;
            await DisplayAlert("Help & Support", "Help & Support page coming soon!", "OK");
        }

        private async void LogoutItemTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = false;
            bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (answer)
            {
                // Navigate back to login page
                Application.Current.MainPage = new LoginPage();
            }
        }

        // Hamburger button tap handler
        private void OnHamburgerButtonTapped(object sender, TappedEventArgs e)
        {
            _menuPopup.IsVisible = true;
        }

        private void InitializeUserProfile()
        {
            try
            {
                Debug.WriteLine("HomePage: Initializing user profile");

                // Check if App.User exists
                if (App.User == null)
                {
                    Debug.WriteLine("HomePage: App.User is null, creating empty profile");
                    // Create a placeholder user to prevent crashes
                    App.User = new Domain.User
                    {
                        Profile = new Domain.Profile
                        {
                            UserName = "User"
                        }
                    };
                }

                // Check if App.User.Profile exists
                if (App.User.Profile == null)
                {
                    Debug.WriteLine("HomePage: App.User.Profile is null, creating empty profile");
                    App.User.Profile = new Domain.Profile
                    {
                        UserName = "User"
                    };
                }

                // Safely update UI with profile data
                try
                {
                    // Load profile image if available
                    if (!string.IsNullOrEmpty(App.User.Profile.ImageURL))
                    {
                        try
                        {
                            Debug.WriteLine("HomePage: Setting profile image");
                            ProfileImage.Source = App.User.Profile.ImageURL;
                            ProfileImage.IsVisible = true;
                        }
                        catch (Exception imgEx)
                        {
                            Debug.WriteLine($"HomePage: Error loading profile image: {imgEx.Message}");
                            ProfileImage.IsVisible = false;
                        }
                    }
                    else
                    {
                        // No image URL is available, ensure image is not visible
                        ProfileImage.IsVisible = false;
                    }
                }
                catch (Exception uiEx)
                {
                    Debug.WriteLine($"HomePage: Error updating UI: {uiEx.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected errors
                Debug.WriteLine($"HomePage: Error in InitializeUserProfile: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Debug.WriteLine("HomePage: OnAppearing called");

                // Ensure UI is properly initialized
                InitializeUserProfile();

                // Make sure Shell navigation is properly set up
                EnsureShellConfiguration();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error in OnAppearing: {ex.Message}");
            }
        }

        private void EnsureShellConfiguration()
        {
            try
            {
                // If we're in a Shell environment
                if (Shell.Current != null)
                {
                    // Make sure navigation bar is visible
                    Shell.SetNavBarIsVisible(this, true);

                    // Try to set this page as the current tab
                    if (Shell.Current.Items.Count > 0)
                    {
                        var item = Shell.Current.Items.FirstOrDefault(i => i.Route?.Contains("HomePage") == true);
                        if (item != null)
                        {
                            Debug.WriteLine("HomePage: Setting current Shell item to HomePage");
                            Shell.Current.CurrentItem = item;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error configuring Shell: {ex.Message}");
            }
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            // We're already on the home page, so no navigation needed
            Debug.WriteLine("HomePage: Already on HomePage, no navigation needed");
        }

        // Unified navigation method to handle all navigation with consistent approach
        private async Task NavigateToPage(string routeName)
        {
            try
            {
                Debug.WriteLine($"HomePage: Attempting to navigate to {routeName}");
                string route = $"//{routeName}";

                // Try multiple navigation methods in order of preference
                Exception lastException = null;

                // Method 1: Use Shell Navigation if available (preferred)
                if (Shell.Current != null)
                {
                    try
                    {
                        Debug.WriteLine($"HomePage: Using Shell.GoToAsync to navigate to {route}");
                        await Shell.Current.GoToAsync(route);
                        return;
                    }
                    catch (Exception shellEx)
                    {
                        lastException = shellEx;
                        Debug.WriteLine($"HomePage: Shell navigation failed: {shellEx.Message}");
                    }
                }

                // Method 2: Try regular page navigation
                try
                {
                    // Create a new instance of the target page
                    var serviceProvider = MauiProgram.CreateMauiApp().Services;
                    Page targetPage = null;

                    // Get the appropriate page based on the route name
                    switch (routeName)
                    {
                        case "StatsPage":
                            targetPage = serviceProvider.GetService<StatsPage>() ?? new StatsPage();
                            break;
                        case "FindRunsPage":
                            targetPage = serviceProvider.GetService<FindRunsPage>() ?? new FindRunsPage();
                            break;
                        case "HoopersPage":
                            targetPage = serviceProvider.GetService<HoopersPage>() ?? new HoopersPage();
                            break;
                        case "PostsPage":
                            targetPage = serviceProvider.GetService<PostsPage>() ?? new PostsPage();
                            break;
                        case "ShopPage":
                            targetPage = serviceProvider.GetService<ShopPage>() ?? new ShopPage();
                            break;
                        case "EditProfilePage":
                            targetPage = serviceProvider.GetService<EditProfilePage>() ?? new EditProfilePage();
                            break;
                        default:
                            targetPage = new HomePage(); // Default fallback to HomePage
                            break;
                    }

                    if (targetPage != null && Navigation != null)
                    {
                        Debug.WriteLine($"HomePage: Using Navigation.PushAsync to navigate to {routeName}");
                        await Navigation.PushAsync(targetPage);
                        return;
                    }
                }
                catch (Exception navEx)
                {
                    lastException = navEx;
                    Debug.WriteLine($"HomePage: Navigation.PushAsync failed: {navEx.Message}");
                }

                // Method 3: Use NavigationHelper as a fallback
                try
                {
                    Debug.WriteLine($"HomePage: Using NavigationHelper to navigate to {route}");
                    await NavigationHelper.NavigateTo(this, route);
                    return;
                }
                catch (Exception helperEx)
                {
                    lastException = helperEx;
                    Debug.WriteLine($"HomePage: NavigationHelper failed: {helperEx.Message}");
                }

                // Method A4: Use DirectNavigationHelper as a last resort
                try
                {
                    Debug.WriteLine($"HomePage: Using DirectNavigationHelper as last resort");
                    await DirectNavigationHelper.GoToPageAsync(routeName);
                    return;
                }
                catch (Exception directEx)
                {
                    lastException = directEx;
                    Debug.WriteLine($"HomePage: DirectNavigationHelper failed: {directEx.Message}");
                }

                // If all navigation methods failed, show an error to the user
                if (lastException != null)
                {
                    Debug.WriteLine($"HomePage: All navigation methods failed: {lastException.Message}");
                    await DisplayAlert("Navigation Error",
                        $"Could not navigate to {routeName}. Please try again.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Unhandled error navigating to {routeName}: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    $"Could not navigate to {routeName}. Please try again or restart the app.",
                    "OK");
            }
        }

        // All card click handlers now use the unified navigation method
        private async void OnStatsClicked(object sender, EventArgs e)
        {
            await NavigateToPage("StatsPage");
        }

        private async void OnFindGamesClicked(object sender, EventArgs e)
        {
            await NavigateToPage("FindRunsPage");
        }

        private async void OnHoopersClicked(object sender, EventArgs e)
        {
            await NavigateToPage("HoopersPage");
        }

        private async void OnTeamsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Teams", "Teams feature coming soon!", "OK");
        }

        private async void OnPostsClicked(object sender, EventArgs e)
        {
            await NavigateToPage("PostsPage");
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            await NavigateToPage("ShopPage");
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await NavigateToPage("EditProfilePage");
        }

        // Navigation bar handlers
        private async void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            await NavigateToPage("PostsPage");
        }

        private void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            // We're already on HomePage, so no navigation needed
            Debug.WriteLine("Already on HomePage, no navigation needed");
        }

        private async void OnMessagesNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnMessagesNavigationClicked - Attempting navigation to MessagesPage");
                await DisplayAlert("Messages", "Messages feature coming soon!", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error: {ex.Message}");
            }
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            // Use Shell's flyout menu instead of custom menu popup
            try
            {
                if (Shell.Current != null)
                {
                    Shell.Current.FlyoutIsPresented = true;
                    Debug.WriteLine("HomePage: Showing Shell flyout menu");
                }
                else
                {
                    DisplayAlert("Menu", "Menu is not available in this context", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error showing menu: {ex.Message}");
                DisplayAlert("Menu", "Could not display menu", "OK");
            }
        }
    }
}