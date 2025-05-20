using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Diagnostics;
using UltimateHoopers.Helpers; // Add this to import NavigationHelper

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
                Debug.WriteLine("PostsPage: OnHomeClicked - using DirectNavigationHelper");

                // Use the simpler, more direct navigation helper
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PostsPage: Error navigating to HomePage: {ex.Message}");

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
                // Check if we're already on the PostsPage
                Page currentPage = null;

                if (Shell.Current != null)
                {
                    currentPage = Shell.Current.CurrentPage;
                }
                else if (Application.Current?.MainPage != null)
                {
                    currentPage = Application.Current.MainPage;
                }

                // If we're already on PostsPage, do nothing
                if (currentPage is StatsPage)
                {
                    Console.WriteLine("Already on PostsPage, skipping navigation");
                    return;
                }

                Console.WriteLine("Navigating to StatsPage");
                await NavigationHelper.NavigateTo(this, "//StatsPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to PostsPage: {ex.Message}");
            }
        }

        private async void OnFindGamesClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//FindRunsPage");
        }

        private async void OnHoopersClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//HoopersPage");
        }

        private async void OnTeamsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Teams", "Teams feature coming soon!", "OK");
        }

        private async void OnPostsClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//PostsPage");
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//ShopPage");
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//EditProfilePage");
        }

        private async void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                // Check if we're already on the PostsPage
                Page currentPage = null;

                if (Shell.Current != null)
                {
                    currentPage = Shell.Current.CurrentPage;
                }
                else if (Application.Current?.MainPage != null)
                {
                    currentPage = Application.Current.MainPage;
                }

                // If we're already on PostsPage, do nothing
                if (currentPage is PostsPage)
                {
                    Console.WriteLine("Already on PostsPage, skipping navigation");
                    return;
                }

                Console.WriteLine("Navigating to PostsPage");
                await NavigationHelper.NavigateTo(this, "//PostsPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to PostsPage: {ex.Message}");
            }
        }

        private void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            // We're already on HomePage, so do nothing
            Console.WriteLine("Already on HomePage, no navigation needed");
        }

        private async void OnMessagesNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await NavigationHelper.NavigateTo(this, "//MessagesPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to MessagesPage: {ex.Message}");
                await DisplayAlert("Navigation", "Messages feature coming soon!", "OK");
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
                }
                else
                {
                    DisplayAlert("Menu", "Menu is not available in this context", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing menu: {ex.Message}");
                DisplayAlert("Menu", "Could not display menu", "OK");
            }
        }
    }
}