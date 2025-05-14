using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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

        private async void OnHoopersClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Hoopers", "Hoopers feature coming soon!", "OK");
        }

        private async void OnTeamsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Teams", "Teams feature coming soon!", "OK");
        }

        private async void OnPostsClicked(object sender, EventArgs e)
        {
            // Navigate to posts page
            await Shell.Current.GoToAsync("//PostsPage");
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Shop", "Shop feature coming soon!", "OK");
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Profile", "Profile feature coming soon!", "OK");
        }

        private async void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to posts page
            await Shell.Current.GoToAsync("//PostsPage");
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            // Use Shell's flyout menu instead of custom menu popup
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}