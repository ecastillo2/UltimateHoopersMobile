using AndroidX.AppCompat.View.Menu;
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
            // Toggle the menu popup instead of Shell.FlyoutIsPresented
            MenuPopup.IsVisible = !MenuPopup.IsVisible;
        }

        private void OnCloseMenu(object sender, EventArgs e)
        {
            MenuPopup.IsVisible = false;
        }

        private async void OnProfileMenuItemClicked(object sender, EventArgs e)
        {
            MenuPopup.IsVisible = false;
            await DisplayAlert("Profile", "Profile page coming soon!", "OK");
        }

        private async void OnSettingsMenuItemClicked(object sender, EventArgs e)
        {
            MenuPopup.IsVisible = false;
            await DisplayAlert("Settings", "Settings page coming soon!", "OK");
        }

        private async void OnNotificationsMenuItemClicked(object sender, EventArgs e)
        {
            MenuPopup.IsVisible = false;
            await DisplayAlert("Notifications", "Notifications page coming soon!", "OK");
        }

        private async void OnHelpMenuItemClicked(object sender, EventArgs e)
        {
            MenuPopup.IsVisible = false;
            await DisplayAlert("Help & Support", "Help & Support page coming soon!", "OK");
        }

        private async void OnLogoutMenuItemClicked(object sender, EventArgs e)
        {
            MenuPopup.IsVisible = false;
            bool answer = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (answer)
            {
                // Navigate back to the login page
                Application.Current.MainPage = new LoginPage();
                await DisplayAlert("Logout", "You have been logged out", "OK");
            }
        }
    }
}