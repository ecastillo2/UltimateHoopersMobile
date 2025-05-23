using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using UltimateHoopers.Models;
using UltimateHoopers.ViewModels;
using UltimateHoopers.Converter;

namespace UltimateHoopers.Pages
{
    public partial class RunDetailsPage : ContentPage
    {
        private RunDetailsViewModel _viewModel;

        public RunDetailsPage(RunDto run)
        {
            InitializeComponent();

            // Register the InvertBoolConverter if not already registered
            if (!Resources.TryGetValue("InvertBoolConverter", out _))
            {
                Resources.Add("InvertBoolConverter", new InvertBoolConverter());
            }

            // Create the view model with the run parameter
            _viewModel = new RunDetailsViewModel(run);
            BindingContext = _viewModel;

            // Set a reasonable max height for the players collection
            UpdatePlayersCollectionHeight();

            // Set up event handlers for player interaction
            SetupPlayerInteraction();
        }

        // Parameterless constructor for XAML preview and DI
        public RunDetailsPage()
        {
            InitializeComponent();

            // Register the InvertBoolConverter if not already registered
            if (!Resources.TryGetValue("InvertBoolConverter", out _))
            {
                Resources.Add("InvertBoolConverter", new InvertBoolConverter());
            }

            // Create a default run if no parameter is provided
            var defaultRun = new RunDto
            {
                Id = "default",
                Name = "Sample Run",
                Location = "Sample Location",
                Address = "123 Sample St",
                Date = DateTime.Now.AddDays(1),
                Time = "6:00 PM - 8:00 PM",
                HostName = "Sample Host",
                SkillLevel = "All Levels",
                GameType = "5-on-5",
                IsPublic = true,
                Description = "Sample run description",
                PlayerLimit = 10,
                CurrentPlayerCount = 5,
                Cost = 0,
                Distance = 1.5
            };

            // Make sure the sample run has a Players collection
            if (defaultRun.Players == null)
            {
                defaultRun.Players = new System.Collections.ObjectModel.ObservableCollection<Player>();

                // Add a few sample players
                defaultRun.Players.Add(new Player
                {
                    Id = "host123",
                    Name = "Sample Host",
                    Username = "@samplehost",
                    IsHost = true,
                    HasJoined = true
                });

                defaultRun.Players.Add(new Player
                {
                    Id = "player1",
                    Name = "John Smith",
                    Username = "@johnsmith",
                    IsHost = false,
                    HasJoined = true
                });
            }

            _viewModel = new RunDetailsViewModel(defaultRun);
            BindingContext = _viewModel;

            // Set a reasonable max height for the players collection
            UpdatePlayersCollectionHeight();

            // Set up event handlers for player interaction
            SetupPlayerInteraction();
        }

        private void SetupPlayerInteraction()
        {
            // Add event handler for tapping on player items
            PlayersCollectionView.SelectionChanged += (sender, e) => {
                if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
                {
                    var selectedPlayer = e.CurrentSelection[0] as Player;
                    if (selectedPlayer != null && _viewModel.ViewPlayerProfileCommand.CanExecute(selectedPlayer))
                    {
                        _viewModel.ViewPlayerProfileCommand.Execute(selectedPlayer);
                    }

                    // Reset selection
                    PlayersCollectionView.SelectedItem = null;
                }
            };
        }

        private async void OnBackClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating back: {ex.Message}");
            }
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//HomePage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to home: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to home page", "OK");
            }
        }

        private async void OnSquadNavigationClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Squad", "Squad page coming soon!", "OK");
        }

        private async void OnSettingsNavigationClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Settings", "Settings page coming soon!", "OK");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh run details when page appears
            _viewModel?.RefreshRunDetails();

            // Update the height of the players collection
            UpdatePlayersCollectionHeight();

            // Force refresh of joined players list
            _viewModel?.RefreshJoinedPlayersList();
        }

        private void UpdatePlayersCollectionHeight()
        {
            try
            {
                // Adjust the height of the players collection based on player count
                int playerCount = _viewModel?.JoinedPlayers?.Count ?? 0;

                // Log the player count for debugging
                Debug.WriteLine($"Updating players collection height - Player count: {playerCount}");

                // Set minimum height to show at least 3 players
                double minHeight = 150;

                // Calculate height based on player count (approximately 68 pixels per player with extra padding)
                double calculatedHeight = Math.Max(minHeight, playerCount * 68);

                // Set a maximum height to prevent it from taking up too much screen space
                double maxHeight = 300;

                // Apply the calculated height within the min-max range
                PlayersCollectionView.HeightRequest = Math.Min(calculatedHeight, maxHeight);

                Debug.WriteLine($"Updated players collection height to {PlayersCollectionView.HeightRequest}px for {playerCount} players");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating players collection height: {ex.Message}");
            }
        }
    }
}