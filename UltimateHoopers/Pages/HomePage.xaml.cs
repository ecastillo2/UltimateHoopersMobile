using Microsoft.Maui.Controls;

namespace UltimateHoopers.Pages
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        // CORRECT HANDLERS FOR TAP GESTURE RECOGNIZERS
        private async void OnStatsClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Stats", "Stats feature coming soon!", "OK");
        }

        private async void OnFindGamesClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Find Games", "Find Games feature coming soon!", "OK");
        }

        private async void OnTrainingClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Training", "Training feature coming soon!", "OK");
        }

        private async void OnTeamsClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Teams", "Teams feature coming soon!", "OK");
        }

        private async void OnProfileClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Profile", "Profile feature coming soon!", "OK");
        }

        private void OnMenuClicked(object sender, TappedEventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}