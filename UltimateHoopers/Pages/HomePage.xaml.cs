using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Diagnostics;
using UltimateHoopers.Helpers;
using UltimateHoopers.Pages;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            InitializeUserProfile();
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