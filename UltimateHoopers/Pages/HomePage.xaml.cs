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
            InitializeUserProfile();
        }

        private void InitializeUserProfile()
        {
            //UsernameText.Text = App.User.Profile.UserName;
            //PositionHeightText.Text = $"{App.User.Profile.Position} • {App.User.Profile.Height}";
            //PlayerNumberText.Text = App.User.Profile.PlayerNumber;
            //GamesText.Text = App.User.Profile.TotalGames;
            //RecordText.Text = $"{App.User.Profile.TotalWins} - {App.User.Profile.TotalLosses}";
            //WinPercentageText.Text = App.User.Profile.WinPercentage;



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

        private async void OnStatsClicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to StatsPage
                await Navigation.PushAsync(new StatsPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");

                // Alternative navigation using Shell if direct navigation fails
                try
                {
                    await Shell.Current.GoToAsync("StatsPage");
                }
                catch
                {
                    // Fallback to display an alert if all navigation attempts fail
                    await DisplayAlert("Stats", "Could not navigate to Stats page", "OK");
                }
            }
        }

        private async void OnFindGamesClicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to Find Runs
                await Navigation.PushAsync(new FindRunsPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");

                // Alternative navigation using Shell if direct navigation fails
                try
                {
                    await Shell.Current.GoToAsync("FindRunsPage");
                }
                catch
                {
                    // Fallback to display an alert if all navigation attempts fail
                    await DisplayAlert("Find Runs", "Could not navigate to Finds Runs page", "OK");
                }
            }
        }

        private async void OnHoopersClicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to HoopersPage
                await Navigation.PushAsync(new HoopersPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");

                // Alternative navigation using Shell if direct navigation fails
                try
                {
                    await Shell.Current.GoToAsync("HoopersPage");
                }
                catch
                {
                    // Fallback to display an alert if all navigation attempts fail
                    await DisplayAlert("Hoopers", "Could not navigate to Hoopers page", "OK");
                }
            }
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
            try
            {
                // Navigate to Shop
                await Navigation.PushAsync(new ShopPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");

                // Alternative navigation using Shell if direct navigation fails
                try
                {
                    await Shell.Current.GoToAsync("ShopPage");
                }
                catch
                {
                    // Fallback to display an alert if all navigation attempts fail
                    await DisplayAlert("Shop", "Could not navigate to Finds Runs page", "OK");
                }
            }
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to EditProfilePage
                await Navigation.PushAsync(new EditProfilePage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Profile", "Could not navigate to Edit Profile page", "OK");
            }
        }

        private async void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to posts page
            await Shell.Current.GoToAsync("//PostsPage");
        }

        private async void OnMessagesNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to posts page
            await Shell.Current.GoToAsync("//MessagesPage");
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            // Use Shell's flyout menu instead of custom menu popup
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}