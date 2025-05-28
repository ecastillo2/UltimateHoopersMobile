using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UltimateHoopers.Helpers;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;


namespace UltimateHoopers.Pages
{
    public partial class HoopersPage : ContentPage
    {
        // ViewModel for data binding
        private HoopersViewModel _viewModel;

        public HoopersPage()
        {
            InitializeComponent();

            // Create and initialize the ViewModel
            _viewModel = new HoopersViewModel();
            BindingContext = _viewModel;

            // Load data when page appears
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.LoadHoopersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hoopers: {ex.Message}");
                await DisplayAlert("Error", "Could not load player data. Please try again later.", "OK");
            }
        }

        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("PostsPage: OnHomeClicked - using DirectNavigationHelper");

                // Use the simpler, more direct navigation helper
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PostsPage: Error navigating to HomePage: {ex.Message}");

                // Show an error message to the user
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        // Events from XAML
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterHoopers(e.NewTextValue);
        }

        private void OnFilterGuardsClicked(object sender, EventArgs e)
        {
            _viewModel.FilterByPosition("Guard");
        }

        private void OnFilterForwardsClicked(object sender, EventArgs e)
        {
            _viewModel.FilterByPosition("Forward");
        }

        private void OnFilterCentersClicked(object sender, EventArgs e)
        {
            _viewModel.FilterByPosition("Center");
        }

        private void OnFilterNearbyClicked(object sender, EventArgs e)
        {
            _viewModel.FilterByLocation("Atlanta");
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is HooperViewModel hooper)
            {
                await DisplayAlert("Connect", $"Connecting with @{hooper.Username}", "OK");
            }
        }

        // New handler for tapping on the player card
        private async void OnPlayerCardTapped(object sender, TappedEventArgs e)
        {
            try
            {
                // Get the hooper from the command parameter
                if (e.Parameter is HooperViewModel hooper)
                {
                    Console.WriteLine($"Player card tapped: {hooper.Username}");

                    // Navigate to the player profile page
                    await Navigation.PushAsync(new PlayerProfilePage(hooper));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling card tap: {ex.Message}");
                await DisplayAlert("Error", "Could not load player profile. Please try again.", "OK");
            }
        }
    }

    public class HoopersViewModel : BindableObject
    {
        private ObservableCollection<HooperViewModel> _allHoopers = new ObservableCollection<HooperViewModel>();
        private ObservableCollection<HooperViewModel> _filteredHoopers = new ObservableCollection<HooperViewModel>();
        private bool _isLoading;

        public ObservableCollection<HooperViewModel> FilteredHoopers
        {
            get => _filteredHoopers;
            private set
            {
                _filteredHoopers = value;
                OnPropertyChanged();
            }
        }

        public bool IsNotLoading
        {
            get => !_isLoading;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }

        public async Task LoadHoopersAsync()
        {
            try
            {
                IsLoading = true;

                // Add a small delay to ensure the loading indicator is visible
                await Task.Delay(300);

                // Try to get profile service from DI
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                var profileService = serviceProvider.GetService<IProfileService>();

                if (profileService == null)
                {
                    // Fallback if service is not available through DI
                    profileService = new ProfileService();
                }

                // Load profiles
                var profiles = await profileService.GetProfilesWithCursor();

                // Convert to view models
                _allHoopers.Clear();
                if (profiles != null)
                {
                    foreach (var profile in profiles)
                    {
                        try
                        {
                            var hooper = new HooperViewModel
                            {
                                Username = profile.UserName ?? "",
                                DisplayName = profile.UserName ?? "Unknown Player",
                                Position = profile.Position ?? "Unknown",
                                Location = profile.City ?? "Unknown Location",
                                Rank = profile?.Ranking,
                                GamesPlayed = int.TryParse(profile.GameStatistics.TotalGames.ToString(), out int games) ? games : 0,
                                Record = $"{profile.GameStatistics.TotalWins.ToString() ?? "0"}-{profile.GameStatistics.TotalLosses.ToString() ?? "0"}",
                                WinPercentage = profile.GameStatistics.WinPercentage,
                                Rating = profile.StarRating,
                                ProfileImage = profile.ImageURL,
                                StyleOfPlay = profile.ScoutingReport != null ? profile.ScoutingReport.PlayingStyle ?? "Unknown Player" : "Unknown Player",
                            };

                            // Initialize computed properties
                            hooper.InitProperties();

                            _allHoopers.Add(hooper);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error converting profile to hooper model: {ex.Message}");
                        }
                    }
                }

                // Initialize filtered hoopers with all hoopers
                FilteredHoopers = new ObservableCollection<HooperViewModel>(_allHoopers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profiles: {ex.Message}");
                throw; // Let the page handle this exception
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterHoopers(string searchText)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Reset to show all hoopers
                FilteredHoopers = new ObservableCollection<HooperViewModel>(_allHoopers);
                return;
            }

            // Remove @ symbol if present
            searchText = searchText.TrimStart('@').ToLower();

            // Filter by username
            var filtered = _allHoopers.Where(h => h.Username.ToLower().Contains(searchText)).ToList();
            FilteredHoopers = new ObservableCollection<HooperViewModel>(filtered);
        }

        public void FilterByPosition(string position)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            var filtered = _allHoopers.Where(h => h.Position.Contains(position)).ToList();
            FilteredHoopers = new ObservableCollection<HooperViewModel>(filtered);
        }

        public void FilterByLocation(string location)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            var filtered = _allHoopers.Where(h => h.Location.Contains(location)).ToList();
            FilteredHoopers = new ObservableCollection<HooperViewModel>(filtered);
        }
    }
}