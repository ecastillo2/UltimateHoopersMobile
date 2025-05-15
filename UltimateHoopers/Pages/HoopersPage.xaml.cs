using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UltimateHoopers.Pages
{
    public partial class HoopersPage : ContentPage
    {
        // Collection of players to be displayed and filtered
        private ObservableCollection<HooperModel> _allHoopers;
        private ObservableCollection<HooperModel> _filteredHoopers;
        private VerticalStackLayout _playersContainer;

        public HoopersPage()
        {
            InitializeComponent();

            // Initialize player data
            InitializePlayerData();

            // Get reference to players container
            _playersContainer = this.FindByName<VerticalStackLayout>("PlayersContainer");

            // Set the initial data source and display players
            DisplayPlayers();
        }

        private void InitializePlayerData()
        {
            _allHoopers = new ObservableCollection<HooperModel>
            {
                new HooperModel
                {
                    Username = "mjohnson21",
                    DisplayName = "Michael Johnson",
                    Position = "Point Guard",
                    Location = "Los Angeles, CA",
                    Rank = 2,
                    GamesPlayed = 85,
                    Record = "36-12",
                    WinPercentage = "75%",
                    Rating = 4.8
                },
                new HooperModel
                {
                    Username = "sthompson",
                    DisplayName = "Sarah Thompson",
                    Position = "Shooting Guard",
                    Location = "New York, NY",
                    Rank = 1,
                    GamesPlayed = 92,
                    Record = "45-22",
                    WinPercentage = "67%",
                    Rating = 4.9
                },
                new HooperModel
                {
                    Username = "marcusw",
                    DisplayName = "Marcus Williams",
                    Position = "Forward",
                    Location = "Chicago, IL",
                    Rank = 5,
                    GamesPlayed = 67,
                    Record = "28-15",
                    WinPercentage = "65%",
                    Rating = 4.2
                }
            };

            // Add more sample players
            for (int i = 0; i < 10; i++)
            {
                _allHoopers.Add(new HooperModel
                {
                    Username = $"player{i}",
                    DisplayName = $"Player {i}",
                    Position = i % 3 == 0 ? "Guard" : (i % 3 == 1 ? "Forward" : "Center"),
                    Location = "Atlanta, GA",
                    Rank = i + 6,
                    GamesPlayed = 50 + i * 3,
                    Record = $"{20 + i}-{10 + i}",
                    WinPercentage = $"{60 + i}%",
                    Rating = Math.Round(3.5 + (i * 0.1), 1)
                });
            }

            // Initialize filtered list with all players
            _filteredHoopers = new ObservableCollection<HooperModel>(_allHoopers);
        }

        private void DisplayPlayers()
        {
            // In a real app, you would use data binding with a CollectionView
            // For this example, we're demonstrating how the scrolling would work

            // When implementing with a CollectionView, you would simply do:
            // CollectionView.ItemsSource = _filteredHoopers;
        }

        // Handle search text changes
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower() ?? "";

            // Remove @ symbol if user entered it at the beginning
            if (searchText.StartsWith("@"))
            {
                searchText = searchText.Substring(1);
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // If search is empty, show all players
                _filteredHoopers.Clear();
                foreach (var player in _allHoopers)
                {
                    _filteredHoopers.Add(player);
                }
            }
            else
            {
                // Filter players ONLY by username
                _filteredHoopers.Clear();
                foreach (var player in _allHoopers.Where(p =>
                    p.Username.ToLower().Contains(searchText)))
                {
                    _filteredHoopers.Add(player);
                }
            }

            // Update the display
            // In a real app, the CollectionView would automatically update
        }

        // Filter methods
        private void OnFilterGuardsClicked(object sender, EventArgs e)
        {
            FilterByPosition("Guard");
        }

        private void OnFilterForwardsClicked(object sender, EventArgs e)
        {
            FilterByPosition("Forward");
        }

        private void OnFilterCentersClicked(object sender, EventArgs e)
        {
            FilterByPosition("Center");
        }

        private void OnFilterNearbyClicked(object sender, EventArgs e)
        {
            // In a real app, this would filter by geolocation
            // For now, just filter to players in a specific location
            FilterByLocation("Atlanta, GA");
        }

        private void FilterByPosition(string position)
        {
            _filteredHoopers.Clear();
            foreach (var player in _allHoopers.Where(p => p.Position.Contains(position)))
            {
                _filteredHoopers.Add(player);
            }

            // In a real app, the UI would automatically update through data binding
        }

        private void FilterByLocation(string location)
        {
            _filteredHoopers.Clear();
            foreach (var player in _allHoopers.Where(p => p.Location.Contains(location)))
            {
                _filteredHoopers.Add(player);
            }

            // In a real app, the UI would automatically update through data binding
        }
        // Navigation methods
        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            // Navigate to home page
            // Example: await Navigation.PushAsync(new HomePage());
        }
        // Model class for Hooper data
        public class HooperModel
        {
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Position { get; set; }
            public string Location { get; set; }
            public int Rank { get; set; }
            public int GamesPlayed { get; set; }
            public string Record { get; set; }
            public string WinPercentage { get; set; }
            public double Rating { get; set; }
        }
    }
}