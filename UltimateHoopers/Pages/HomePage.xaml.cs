using System;

namespace UltimateHoopers.Pages
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Stats", "Stats feature coming soon!", "OK");
        }

        private async void OnFindGamesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Find Games", "Find Games feature coming soon!", "OK");
        }

        private async void OnTrainingClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Training", "Training feature coming soon!", "OK");
        }

        private async void OnTeamsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Teams", "Teams feature coming soon!", "OK");
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Profile", "Profile feature coming soon!", "OK");
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            // Toggle the Shell Flyout menu
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}