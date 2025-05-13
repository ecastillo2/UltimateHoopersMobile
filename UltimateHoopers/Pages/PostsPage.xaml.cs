using Microsoft.Maui.Controls;

namespace UltimateHoopers.Pages
{
    public partial class PostsPage : ContentPage
    {
        public PostsPage()
        {
            InitializeComponent();
        }

        private async void OnProfileClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Profile", "Profile feature coming soon!", "OK");
        }

        private void OnMenuClicked(object sender, TappedEventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }

        private async void OnHomeClicked(object sender, TappedEventArgs e)
        {
            // Navigate back to the HomePage
            await Shell.Current.GoToAsync("//HomePage");
        }

        private async void OnExploreClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Explore", "Explore feature coming soon!", "OK");
        }

        private async void OnCreatePostClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("New Post", "Create new post feature coming soon!", "OK");
        }

        private async void OnActivityClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Activity", "Activity feed feature coming soon!", "OK");
        }

        private async void OnScheduleClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Schedule", "Schedule feature coming soon!", "OK");
        }

        private async void OnSettingsClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Settings", "Settings feature coming soon!", "OK");
        }
    }
}