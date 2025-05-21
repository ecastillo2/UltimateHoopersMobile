using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace UltimateHoopers.Pages
{
    public partial class RunDetailsPage : ContentPage
    {
        // Properties for binding
        public Run Run { get; private set; }
        public ObservableCollection<Player> Players { get; private set; }
        public string FormattedDate => Run.Date.ToString("dddd, MMMM d, yyyy");
        public string GameTypeText => $"{GetGameTypeFromPlayerCount(Run.PlayerLimit)}";
        public string PrivacyText => Run.IsPublic ? "Public (anyone can join)" : "Private (invitation only)";

        public RunDetailsPage(Run run)
        {
            Run = run;
            Players = new ObservableCollection<Player>();

            InitializeComponent();
            InitializePageData();

            // Set the page binding context
            BindingContext = this;
        }

        private void InitializePageData()
        {
            // Load sample player data
            LoadSamplePlayers();
        }

        private void LoadSamplePlayers()
        {
            // Clear existing players
            Players.Clear();

            // Add host as first player
            Players.Add(new Player
            {
                Id = "host123",
                Name = Run.HostName,
                IsHost = true
            });

            // Add some sample players
            var sampleNames = new List<string>
            {
                "LeBron James",
                "Stephen Curry",
                "Kevin Durant",
                "Giannis Antetokounmpo",
                "James Harden",
                "Luka Doncic"
            };

            // Add random subset of players up to the current player count
            var random = new Random();
            var shuffledNames = sampleNames.OrderBy(x => random.Next()).ToList();

            for (int i = 0; i < Math.Min(Run.CurrentPlayerCount - 1, shuffledNames.Count); i++)
            {
                Players.Add(new Player
                {
                    Id = $"player{i}",
                    Name = shuffledNames[i],
                    IsHost = false
                });
            }
        }

        private string GetGameTypeFromPlayerCount(int playerCount)
        {
            return playerCount switch
            {
                10 => "5-on-5 (Full Court)",
                8 => "4-on-4",
                6 => "3-on-3 (Half Court)",
                4 => "2-on-2",
                2 => "1-on-1",
                _ => $"Custom ({playerCount} players)"
            };
        }

        #region Navigation
        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            Navigation.PopToRootAsync();
        }

        private void OnSquadNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to squads page
            // Implementation depends on your navigation structure
        }

        private void OnSettingsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to settings page
            // Implementation depends on your navigation structure
        }
        #endregion

        #region Event Handlers
        private async void OnJoinRunClicked(object sender, EventArgs e)
        {
            // Check if the run is already full
            if (Run.CurrentPlayerCount >= Run.PlayerLimit)
            {
                await DisplayAlert("Run Full", "This run is already at capacity. Would you like to join the waitlist?", "Join Waitlist", "Cancel");
                return;
            }

            // Confirm joining
            bool confirmed = await DisplayAlert(
                "Join Run",
                $"Are you sure you want to join '{Run.Name}' on {FormattedDate}?",
                "Join", "Cancel");

            if (confirmed)
            {
                // Simulate API call
                await SimulateLoadingAsync("Joining run...");

                // Add the current user to the players list
                Players.Add(new Player
                {
                    Id = "currentUser",
                    Name = "You",
                    IsHost = false
                });

                // Update player count
                Run.CurrentPlayerCount++;
                OnPropertyChanged(nameof(Run));

                // Show success message
                await DisplayAlert(
                    "Success",
                    $"You've joined '{Run.Name}'! See you on the court!",
                    "OK");
            }
        }

        private async void OnShareRunClicked(object sender, EventArgs e)
        {
            // Show share options
            string action = await DisplayActionSheet(
                "Share Run",
                "Cancel",
                null,
                "Share to Social Media",
                "Share via Message",
                "Copy Link");

            if (action == "Share to Social Media" || action == "Share via Message" || action == "Copy Link")
            {
                await DisplayAlert("Share", $"You selected: {action}", "OK");

                // In a real app, you would implement the actual sharing functionality here
                // using the Share plugin or platform-specific sharing APIs
            }
        }
        #endregion

        #region Helper Methods
        private async Task SimulateLoadingAsync(string message)
        {
            // Show loading indicator
            await DisplayAlert("Processing", message, "Please wait");

            // Simulate delay
            await Task.Delay(1000);
        }
        #endregion
    }

    // Player model class
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; }
    }
}