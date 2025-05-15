using Microsoft.Maui.Controls;
using System;

namespace UltimateHoopers.Pages
{
    public partial class StatsPage : ContentPage
    {
        public StatsPage()
        {
            InitializeComponent();
        }

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