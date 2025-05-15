using Microsoft.Maui.Controls;
using System;

namespace UltimateHoopers.Pages
{
    public partial class StatsPage : ContentPage
    {
        public StatsPage()
        {
            InitializeComponent();
            InitializeUserProfile();
        }

        private void InitializeUserProfile()
        {
            UsernameText.Text = App.User.Profile.UserName;
            PositionHeightText.Text = $"{App.User.Profile.Position} • {App.User.Profile.Height}";
            PlayerNumberText.Text = App.User.Profile.PlayerNumber;
            GamesText.Text = App.User.Profile.TotalGames;
            RecordText.Text = $"{App.User.Profile.TotalWins} - {App.User.Profile.TotalLosses}";
            WinPercentageText.Text = App.User.Profile.WinPercentage;



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
            // Navigate to home page
            await Shell.Current.GoToAsync("//HomePage");
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