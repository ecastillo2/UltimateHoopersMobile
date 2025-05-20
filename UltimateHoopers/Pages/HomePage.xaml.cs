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
            try
            {
                Debug.WriteLine("HomePage: OnHomeClicked - using DirectNavigationHelper");

                // Use the simpler, more direct navigation helper
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error navigating to HomePage: {ex.Message}");

                // Show an error message to the user
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnStatsClicked - Starting navigation to Stats page");

                // Try multiple navigation methods in order of preference

                // Method 1: Use Shell Navigation if available
                if (Shell.Current != null)
                {
                    try
                    {
                        Debug.WriteLine("HomePage: Using Shell.GoToAsync to navigate to StatsPage");
                        await Shell.Current.GoToAsync("//StatsPage");
                        return;
                    }
                    catch (Exception shellEx)
                    {
                        Debug.WriteLine($"HomePage: Shell navigation failed: {shellEx.Message}");
                        // Fall through to next method
                    }
                }

                // Method 2: Try regular page navigation if shell navigation failed
                try
                {
                    Debug.WriteLine("HomePage: Using Navigation.PushAsync to navigate to StatsPage");
                    // Create a new instance of StatsPage
                    var serviceProvider = MauiProgram.CreateMauiApp().Services;
                    var statsPage = serviceProvider.GetService<StatsPage>() ?? new StatsPage();

                    await Navigation.PushAsync(statsPage);
                    return;
                }
                catch (Exception navEx)
                {
                    Debug.WriteLine($"HomePage: Navigation.PushAsync failed: {navEx.Message}");
                    // Fall through to next method
                }

                // Method 3: Use NavigationHelper as a fallback
                try
                {
                    Debug.WriteLine("HomePage: Using NavigationHelper to navigate to StatsPage");
                    await NavigationHelper.NavigateTo(this, "//StatsPage");
                    return;
                }
                catch (Exception helperEx)
                {
                    Debug.WriteLine($"HomePage: NavigationHelper failed: {helperEx.Message}");
                    // Fall through to final method
                }

                // Method 4: Last resort - direct page setting
                Debug.WriteLine("HomePage: Using DirectNavigationHelper as last resort");
                await DirectNavigationHelper.GoToPageAsync("StatsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Unhandled error navigating to StatsPage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to Stats page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnFindGamesClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnStatsClicked - Starting navigation to Stats page");

                // Try multiple navigation methods in order of preference

                // Method 1: Use Shell Navigation if available
                if (Shell.Current != null)
                {
                    try
                    {
                        Debug.WriteLine("HomePage: Using Shell.GoToAsync to navigate to StatsPage");
                        await Shell.Current.GoToAsync("//FindRunsPage");
                        return;
                    }
                    catch (Exception shellEx)
                    {
                        Debug.WriteLine($"HomePage: Shell navigation failed: {shellEx.Message}");
                        // Fall through to next method
                    }
                }

                // Method 2: Try regular page navigation if shell navigation failed
                try
                {
                    Debug.WriteLine("HomePage: Using Navigation.PushAsync to navigate to StatsPage");
                    // Create a new instance of StatsPage
                    var serviceProvider = MauiProgram.CreateMauiApp().Services;
                    var statsPage = serviceProvider.GetService<FindRunsPage>() ?? new FindRunsPage();

                    await Navigation.PushAsync(statsPage);
                    return;
                }
                catch (Exception navEx)
                {
                    Debug.WriteLine($"HomePage: Navigation.PushAsync failed: {navEx.Message}");
                    // Fall through to next method
                }

                // Method 3: Use NavigationHelper as a fallback
                try
                {
                    Debug.WriteLine("HomePage: Using NavigationHelper to navigate to FindRunsPage");
                    await NavigationHelper.NavigateTo(this, "//FindRunsPage");
                    return;
                }
                catch (Exception helperEx)
                {
                    Debug.WriteLine($"HomePage: NavigationHelper failed: {helperEx.Message}");
                    // Fall through to final method
                }

                // Method 4: Last resort - direct page setting
                Debug.WriteLine("HomePage: Using DirectNavigationHelper as last resort");
                await DirectNavigationHelper.GoToPageAsync("FindRunsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Unhandled error navigating to StatsPage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to Stats page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnHoopersClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnHoopersClicked - Starting navigation to HoopersPage");
                await Shell.Current.GoToAsync("//HoopersPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error navigating to HoopersPage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to Hoopers page", "OK");
            }
        }

        private async void OnTeamsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Teams", "Teams feature coming soon!", "OK");
        }

        private async void OnPostsClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnPostsClicked - Starting navigation to PostsPage");
                await Shell.Current.GoToAsync("//PostsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error navigating to PostsPage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to Posts page", "OK");
            }
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnShopClicked - Starting navigation to ShopPage");
                await Shell.Current.GoToAsync("//ShopPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error navigating to ShopPage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to Shop page", "OK");
            }
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnProfileClicked - Starting navigation to EditProfilePage");
                await Shell.Current.GoToAsync("//EditProfilePage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error navigating to EditProfilePage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to Profile page", "OK");
            }
        }

        private async void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("HomePage: OnPostsNavigationClicked - Starting navigation to PostsPage");

                // Check if we're already on the PostsPage
                if (Shell.Current?.CurrentPage is PostsPage)
                {
                    Debug.WriteLine("Already on PostsPage, skipping navigation");
                    return;
                }

                await Shell.Current.GoToAsync("//PostsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HomePage: Error navigating to PostsPage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to Posts page", "OK");
            }
        }

        private void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            // We're already on HomePage, so do nothing
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