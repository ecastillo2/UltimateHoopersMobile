using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
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
                // Check if App.User exists
                if (App.User == null)
                {
                    Console.WriteLine("App.User is null. User may not be logged in.");
                    return;
                }

                // Check if App.User.Profile exists
                if (App.User.Profile == null)
                {
                    Console.WriteLine("App.User.Profile is null. User profile hasn't been loaded yet.");
                    return;
                }

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
                        // Optionally set a default image
                        // ProfileImage.Source = "default_profile.png";
                        ProfileImage.IsVisible = false;
                    }
                }
                else
                {
                    // No image URL is available, ensure image is not visible
                    ProfileImage.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected errors
                Console.WriteLine($"Error in InitializeUserProfile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Updated navigation methods using NavigationHelper

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//StatsPage");
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