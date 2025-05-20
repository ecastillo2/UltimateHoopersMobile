using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using UltimateHoopers.Helpers;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class StatsPage : ContentPage
    {
        
        public StatsPage()
        {
            InitializeComponent();
            _ = InitializeUserProfile();
        }

        private async Task InitializeUserProfile()
        {
            UsernameText.Text = App.User.Profile.UserName;
            PositionHeightText.Text = $"{App.User.Profile.Position} • {App.User.Profile.Height}";
            PlayerNumberText.Text = App.User.Profile.PlayerNumber;
            //GamesText.Text = App.User.Profile.TotalGames;
            
            

            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            var profileService = serviceProvider.GetService<IProfileService>();
            if (profileService == null)
            {
                // Fallback if service is not available through DI
                profileService = new ProfileService();
            }

            // Load profiles
            var profile = await profileService.GetProfileByIdAsync(App.User.Profile.ProfileId);
            GamesText.Text = profile.GameStatistics.TotalGames.ToString();
            RecordText.Text = $"{profile.GameStatistics.TotalWins.ToString()} - {profile.GameStatistics.TotalLosses.ToString()}";
            WinPercentageText.Text = profile.GameStatistics.WinPercentage.ToString();

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

        // Add this method to your page's code-behind file


        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
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

        // Action methods
        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Edit Profile", "Profile editing feature coming soon!", "OK");
        }

        private async void OnViewAllGamesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("View All Games", "Game history feature coming soon!", "OK");
        }
    }
}