using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class HoopersPage : ContentPage
    {
        // Collection of players to be displayed and filtered
        private ObservableCollection<HooperModel> _allHoopers;
        private ObservableCollection<HooperModel> _filteredHoopers;
        private readonly ProfileViewModel _viewModel;
        private ActivityIndicator _loadingIndicator;

        public HoopersPage()
        {
            InitializeComponent();

            // Try to get view model from DI
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            var profileService = serviceProvider.GetService<IProfileService>();

            if (profileService != null)
            {
                _viewModel = new ProfileViewModel(profileService);
            }
            else
            {
                // Fallback if service is not available through DI
                _viewModel = new ProfileViewModel(new ProfileService());
            }

            // Set the binding context
            BindingContext = _viewModel;

            // Initialize collections
            _allHoopers = new ObservableCollection<HooperModel>();
            _filteredHoopers = new ObservableCollection<HooperModel>();

            // Create and configure the loading indicator
            _loadingIndicator = new ActivityIndicator
            {
                IsRunning = false,
                Color = (Color)Application.Current.Resources["PrimaryColor"],
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 50,
                WidthRequest = 50,
                Scale = 1.5,
                IsVisible = false
            };

            // Add the loading indicator to the page
            AddLoadingIndicator();
        }

        private void AddLoadingIndicator()
        {
            try
            {
                // Find the ScrollView containing player cards
                var scrollView = this.FindByName<ScrollView>("PlayersScrollView");
                if (scrollView == null)
                {
                    // If we can't find the named ScrollView, try to find it by type and position
                    var contentGrid = (this.Content as Grid);
                    if (contentGrid != null && contentGrid.Children.Count > 1)
                    {
                        var mainGrid = contentGrid.Children[1] as Grid;
                        if (mainGrid != null && mainGrid.Children.Count > 2)
                        {
                            scrollView = mainGrid.Children[2] as ScrollView;
                        }
                    }
                }

                if (scrollView != null && scrollView.Content is VerticalStackLayout stackLayout)
                {
                    // Create a grid to hold both the content and the loading indicator
                    var gridContainer = new Grid
                    {
                        RowDefinitions = new RowDefinitionCollection
                        {
                            new RowDefinition { Height = GridLength.Star }
                        }
                    };

                    // Add the existing content to the grid
                    gridContainer.Add(stackLayout, 0, 0);

                    // Add the loading indicator to the grid
                    gridContainer.Add(_loadingIndicator, 0, 0);

                    // Replace the ScrollView's content with the grid
                    scrollView.Content = gridContainer;
                }
                else
                {
                    Console.WriteLine("Could not find a suitable location to add the loading indicator");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding loading indicator: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Show the loading indicator
                ShowLoading(true);

                // Load profiles from the API when the page appears
                await _viewModel.LoadProfilesAsync();

                // Access the loaded profiles from the ViewModel's Profiles collection
                // and convert them to HooperModel objects if needed
                var hooperModels = ConvertProfilesToHooperModels(_viewModel.Profiles);

                // Store all hoopers for filtering later
                _allHoopers = new ObservableCollection<HooperModel>(hooperModels);

                // Use the filtered items for display
                _filteredHoopers = FilterItems(hooperModels);

                // Update the UI with the filtered players
                UpdatePlayersUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profiles: {ex.Message}");
                await DisplayAlert("Error", "Could not load player data. Please try again later.", "OK");

                // Initialize empty collections if they're null
                if (_allHoopers == null)
                    _allHoopers = new ObservableCollection<HooperModel>();

                if (_filteredHoopers == null)
                    _filteredHoopers = new ObservableCollection<HooperModel>();
            }
            finally
            {
                // Hide the loading indicator when done
                ShowLoading(false);
            }
        }

        // Helper method to show or hide the loading indicator
        private void ShowLoading(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.IsRunning = isLoading;
                _loadingIndicator.IsVisible = isLoading;
            }
        }

        // Helper method to convert Profile objects to HooperModel objects
        private List<HooperModel> ConvertProfilesToHooperModels(ObservableCollection<Domain.Profile> profiles)
        {
            var hooperModels = new List<HooperModel>();

            if (profiles == null || profiles.Count == 0)
            {
                Console.WriteLine("No profiles found to convert");
                return hooperModels; // Return empty list
            }

            foreach (var profile in profiles)
            {
                try
                {
                    hooperModels.Add(new HooperModel
                    {
                        Username = profile.UserName ?? "",
                        DisplayName = profile.UserName ?? "Unknown Player",
                        Position = profile.Position ?? "Unknown",
                        Location = profile.City ?? "Unknown Location",
                        Rank = int.TryParse(profile.Ranking, out int rank) ? rank : 99,
                        GamesPlayed = int.TryParse(profile.TotalGames, out int games) ? games : 0,
                        Record = $"{profile.TotalWins.ToString() ?? "0"}-{profile.TotalLosses.ToString() ?? "0"}",
                        WinPercentage = profile.WinPercentage ?? "0%",
                        Rating = double.TryParse(profile.StarRating, out double rating) ? rating : 0.0,
                        ProfileImage = profile.ImageURL
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting profile to hooper model: {ex.Message}");
                }
            }

            return hooperModels;
        }

        // Helper method to update the UI with the current filtered players
        private void UpdatePlayersUI()
        {
            try
            {
                // Find the ScrollView containing player cards
                var scrollView = this.FindByName<ScrollView>("PlayersScrollView");
                if (scrollView == null)
                {
                    // If we can't find the named ScrollView, try to find it by type and position
                    var contentGrid = (this.Content as Grid);
                    if (contentGrid != null && contentGrid.Children.Count > 1)
                    {
                        var mainGrid = contentGrid.Children[1] as Grid;
                        if (mainGrid != null && mainGrid.Children.Count > 2)
                        {
                            scrollView = mainGrid.Children[2] as ScrollView;
                        }
                    }
                }

                if (scrollView == null)
                {
                    Console.WriteLine("Cannot find the ScrollView to update UI");
                    return;
                }

                // Get the container that holds the content and loading indicator
                var container = scrollView.Content as Grid;
                if (container == null || container.Children.Count == 0)
                {
                    Console.WriteLine("ScrollView's content structure is unexpected");
                    return;
                }

                // Get the VerticalStackLayout that contains player cards
                var stackLayout = container.Children[0] as VerticalStackLayout;
                if (stackLayout == null)
                {
                    Console.WriteLine("Cannot find the VerticalStackLayout for player cards");
                    return;
                }

                // Clear existing content except for the title
                if (stackLayout.Children.Count > 0)
                {
                    // Keep the first child (title) and remove the rest
                    var title = stackLayout.Children[0];
                    stackLayout.Clear();
                    stackLayout.Add(title);
                }

                // If there are no players to display, show a message
                if (_filteredHoopers == null || _filteredHoopers.Count == 0)
                {
                    stackLayout.Add(new Label
                    {
                        Text = "No players found",
                        FontSize = 16,
                        TextColor = (Color)Application.Current.Resources["SecondaryTextColor"],
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                }
                else
                {
                    // Add each player to the UI
                    foreach (var player in _filteredHoopers)
                    {
                        // Create a new player card for each player
                        var playerCard = CreatePlayerCard(player);
                        stackLayout.Add(playerCard);
                    }
                }

                Console.WriteLine($"Updated UI with {_filteredHoopers.Count} players");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating UI: {ex.Message}");
            }
        }

        // Helper method to create a player card frame
        private Frame CreatePlayerCard(HooperModel player)
        {
            // Create the frame that will contain the entire player card
            var frame = new Frame
            {
                BorderColor = (Color)Application.Current.Resources["BorderColor"],
                CornerRadius = 10,
                HasShadow = true,
                Padding = 0,
                BackgroundColor = Colors.White,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Create the grid layout for the player card
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            // Profile picture frame
            var profileFrame = new Frame
            {
                CornerRadius = 35,
                HeightRequest = 70,
                WidthRequest = 70,
                Padding = 0,
                Margin = 15,
                BorderColor = (Color)Application.Current.Resources["PrimaryColor"],
                HasShadow = false
            };

            // Basketball emoji as placeholder
            var profilePic = new Label
            {
                Text = "🏀",
                FontSize = 30,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            profileFrame.Content = profilePic;
            grid.SetRowSpan(profileFrame, 2);
            grid.Add(profileFrame, 0, 0);

            // Name and details layout
            var nameDetailsLayout = new VerticalStackLayout
            {
                Margin = new Thickness(0, 15, 0, 0)
            };

            // Username and rank layout
            var usernameRankLayout = new HorizontalStackLayout();

            // Username
            var usernameLabel = new Label
            {
                Text = $"@{player.Username}",
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"]
            };
            usernameRankLayout.Add(usernameLabel);

            // Rank badge
            var rankFrame = new Frame
            {
                BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"],
                CornerRadius = 10,
                HeightRequest = 20,
                Padding = new Thickness(5, 0),
                Margin = new Thickness(10, 0, 0, 0)
            };

            var rankLayout = new HorizontalStackLayout();

            var hashLabel = new Label
            {
                Text = "#",
                FontSize = 12,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };

            var rankLabel = new Label
            {
                Text = player.Rank.ToString(),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };

            rankLayout.Add(hashLabel);
            rankLayout.Add(rankLabel);
            rankFrame.Content = rankLayout;
            usernameRankLayout.Add(rankFrame);

            nameDetailsLayout.Add(usernameRankLayout);

            // Position and location
            var positionLocationLabel = new Label
            {
                Text = $"{player.Position} • {player.Location}",
                FontSize = 14,
                TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
            };
            nameDetailsLayout.Add(positionLocationLabel);

            grid.Add(nameDetailsLayout, 1, 0);

            // Stats ScrollView
            var statsScrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                Margin = new Thickness(0, 5, 0, 15),
                VerticalOptions = LayoutOptions.Center
            };

            var statsLayout = new HorizontalStackLayout
            {
                Spacing = 15,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0)
            };

            // Games stat
            var gamesStack = new VerticalStackLayout();
            var gamesValueLabel = new Label
            {
                Text = player.GamesPlayed.ToString(),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"]
            };
            var gamesTextLabel = new Label
            {
                Text = "Games",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
            };
            gamesStack.Add(gamesValueLabel);
            gamesStack.Add(gamesTextLabel);
            statsLayout.Add(gamesStack);

            // Record stat
            var recordStack = new VerticalStackLayout();
            var recordValueLabel = new Label
            {
                Text = player.Record,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"]
            };
            var recordTextLabel = new Label
            {
                Text = "Record",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
            };
            recordStack.Add(recordValueLabel);
            recordStack.Add(recordTextLabel);
            statsLayout.Add(recordStack);

            // Win percentage stat
            var winStack = new VerticalStackLayout();
            var winValueLabel = new Label
            {
                Text = player.WinPercentage,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"]
            };
            var winTextLabel = new Label
            {
                Text = "Win %",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
            };
            winStack.Add(winValueLabel);
            winStack.Add(winTextLabel);
            statsLayout.Add(winStack);

            // Rating stat
            var ratingStack = new VerticalStackLayout();
            var ratingContainer = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center
            };
            var ratingValueLabel = new Label
            {
                Text = player.Rating.ToString("0.0"),
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"]
            };
            var starLabel = new Label
            {
                Text = "⭐",
                FontSize = 12,
                TextColor = (Color)Application.Current.Resources["PrimaryColor"],
                Margin = new Thickness(2, 0, 0, 0)
            };
            ratingContainer.Add(ratingValueLabel);
            ratingContainer.Add(starLabel);

            var ratingTextLabel = new Label
            {
                Text = "Rating",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
            };
            ratingStack.Add(ratingContainer);
            ratingStack.Add(ratingTextLabel);
            statsLayout.Add(ratingStack);

            statsScrollView.Content = statsLayout;
            grid.Add(statsScrollView, 1, 1);

            // Connect button
            var connectButton = new Button
            {
                Text = "Connect",
                BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"],
                TextColor = Colors.White,
                CornerRadius = 20,
                HeightRequest = 40,
                WidthRequest = 100,
                Margin = new Thickness(0, 15, 15, 0)
            };

            // Add event handler for connect button
            connectButton.Clicked += (sender, e) => OnConnectButtonClicked(player);

            grid.Add(connectButton, 2, 0);

            frame.Content = grid;
            return frame;
        }

        // Implementation of the missing FilterItems method
        private ObservableCollection<HooperModel> FilterItems(IEnumerable<HooperModel> items)
        {
            if (items == null || !items.Any())
                return new ObservableCollection<HooperModel>();

            // If you want to apply a default filter, you can do so here
            // For example, only show players with a certain minimum rating
            // var filtered = items.Where(p => p.Rating >= 4.0);

            // Or just return all items without filtering
            return new ObservableCollection<HooperModel>(items);
        }

        // Handle search text changes
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            string searchText = e.NewTextValue?.ToLower() ?? "";

            // Remove @ symbol if user entered it at the beginning
            if (searchText.StartsWith("@"))
            {
                searchText = searchText.Substring(1);
            }

            // Show loading indicator during filtering
            ShowLoading(true);

            try
            {
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

                // Update the UI with the filtered players
                UpdatePlayersUI();
            }
            finally
            {
                // Hide loading indicator when done
                ShowLoading(false);
            }
        }

        // Event handler for the Connect button
        private async void OnConnectButtonClicked(HooperModel player)
        {
            await DisplayAlert("Connect", $"Connecting with @{player.Username}", "OK");
            // In a real app, this would send a connection request
        }

        // Filter methods
        private void OnFilterGuardsClicked(object sender, EventArgs e)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            ShowLoading(true);
            try
            {
                FilterByPosition("Guard");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void OnFilterForwardsClicked(object sender, EventArgs e)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            ShowLoading(true);
            try
            {
                FilterByPosition("Forward");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void OnFilterCentersClicked(object sender, EventArgs e)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            ShowLoading(true);
            try
            {
                FilterByPosition("Center");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void OnFilterNearbyClicked(object sender, EventArgs e)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            ShowLoading(true);
            try
            {
                // In a real app, this would filter by geolocation
                // For now, just filter to players in a specific location
                FilterByLocation("Atlanta, GA");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void FilterByPosition(string position)
        {
            _filteredHoopers.Clear();
            foreach (var player in _allHoopers.Where(p => p.Position.Contains(position)))
            {
                _filteredHoopers.Add(player);
            }

            // Update the UI with the filtered players
            UpdatePlayersUI();
        }

        private void FilterByLocation(string location)
        {
            _filteredHoopers.Clear();
            foreach (var player in _allHoopers.Where(p => p.Location.Contains(location)))
            {
                _filteredHoopers.Add(player);
            }

            // Update the UI with the filtered players
            UpdatePlayersUI();
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
            public string ProfileImage { get; set; }
            public double Rating { get; set; }
        }
    }
}