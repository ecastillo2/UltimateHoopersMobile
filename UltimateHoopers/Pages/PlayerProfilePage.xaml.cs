using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Xml;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class PlayerProfilePage : ContentPage
    {
        private readonly string _profileId;
        private HooperViewModel _hooper;

        // Constructor for design time
        public PlayerProfilePage()
        {
            InitializeComponent();
        }

        // Main constructor
        public PlayerProfilePage(string profileId)
        {
            InitializeComponent();
            _profileId = profileId;
        }

        // Constructor that takes a HooperViewModel directly
        public PlayerProfilePage(HooperViewModel hooper)
        {
            InitializeComponent();
            _hooper = hooper;
            _profileId = hooper.ProfileId;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // If we already have the hooper data, use it
                if (_hooper != null)
                {
                    UpdateUI(_hooper);
                }
                // Otherwise load it
                else if (!string.IsNullOrEmpty(_profileId))
                {
                    await LoadProfileData();
                }
                else
                {
                    // No profile ID, show error
                    await DisplayAlert("Error", "No profile information available", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "Could not load profile data", "OK");
            }
        }

        private async Task LoadProfileData()
        {
            try
            {
                // Show loading indicator
                IsBusy = true;

                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                var profileService = serviceProvider.GetService<IProfileService>();

                if (profileService == null)
                {
                    // Create a new service if not available through DI
                    profileService = new ProfileService();
                }

                // Load the profile data
                var profile = await profileService.GetProfileByIdAsync(_profileId);

                if (profile == null)
                {
                    await DisplayAlert("Error", "Profile not found", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                // Convert profile to HooperViewModel
                _hooper = new HooperViewModel
                {
                    ProfileId = profile.ProfileId,
                    Username = profile.UserName ?? "Unknown",
                    DisplayName = profile.UserName ?? "Unknown Player",
                    Position = profile.Position ?? "Unknown",
                    Location = profile.City ?? "Unknown Location",
                    Rank = int.TryParse(profile.Ranking, out int rank) ? rank : 99,
                    GamesPlayed = int.TryParse(profile.TotalGames, out int games) ? games : 0,
                    Record = $"{profile.TotalWins ?? 0}-{profile.TotalLosses ?? 0}",
                    WinPercentage = profile.WinPercentage ?? "0%",
                    Rating = double.TryParse(profile.StarRating, out double rating) ? rating : 0.0,
                    ProfileImage = profile.ImageURL,
                    Height = profile.Height,
                    Weight = profile.Weight,
                    PlayerNumber = profile.PlayerNumber,
                    Bio = profile.Bio
                };

                // Initialize computed properties
                _hooper.InitProperties();

                // Update the UI
                UpdateUI(_hooper);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profile data: {ex.Message}");
                await DisplayAlert("Error", "Failed to load profile data", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateUI(HooperViewModel hooper)
        {
            // Set header information
            UsernameLabel.Text = hooper.UsernameDisplay;
            StatusLabel.Text = "Online Now"; // This would come from the actual online status

            // Set profile information
            NameLabel.Text = hooper.DisplayName;
            PositionLabel.Text = $"{hooper.Position}" + (!string.IsNullOrEmpty(hooper.Height) ? $" • {hooper.Height}" : "");
            LocationLabel.Text = hooper.Location;
            PlayerNumberLabel.Text = hooper.PlayerNumber ?? "N/A";

            // Set stats
            GamesLabel.Text = hooper.GamesPlayed.ToString();
            RecordLabel.Text = hooper.Record;
            WinPercentageLabel.Text = hooper.WinPercentage;
            RatingLabel.Text = hooper.RatingDisplay;


            // Set bio
            BioLabel.Text = hooper.StyleOfPlay;
                

            // Set profile image
            if (hooper.HasValidImage)
            {
                ProfileImage.Source = hooper.ProfileImage;
                ProfileImage.IsVisible = true;
                InitialsLabel.IsVisible = false;
            }
            else
            {
                // Use initials instead
                InitialsLabel.Text = hooper.Initials;
                ProfileBackground.Fill = hooper.InitialsColor;
                InitialsLabel.IsVisible = true;
                ProfileImage.IsVisible = false;
            }
        }

        // Event handlers
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//HomePage");
        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // Show connecting state
                string originalText = button.Text;
                button.Text = "Connecting...";
                button.IsEnabled = false;

                try
                {
                    // Simulate connection process
                    await Task.Delay(1000);

                    // Update button state
                    button.Text = "Connected";
                    button.BackgroundColor = Colors.Green;

                    await DisplayAlert("Success", $"You are now connected with {_hooper.Username}", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection error: {ex.Message}");

                    // Reset button
                    button.Text = originalText;
                    button.IsEnabled = true;

                    await DisplayAlert("Error", "Failed to connect with player", "OK");
                }
            }
        }

        private async void OnMessageClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Coming Soon", "Messaging feature is coming soon!", "OK");
        }

        private async void OnViewAllGamesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Coming Soon", "Game history feature is coming soon!", "OK");
        }
    }
}