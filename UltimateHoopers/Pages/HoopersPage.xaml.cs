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
            await Shell.Current.GoToAsync("//HomePage");
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
                                Rank = int.TryParse(profile.Ranking, out int rank) ? rank : 99,
                                GamesPlayed = int.TryParse(profile.TotalGames, out int games) ? games : 0,
                                Record = $"{profile.TotalWins.ToString() ?? "0"}-{profile.TotalLosses.ToString() ?? "0"}",
                                WinPercentage = profile.WinPercentage ?? "0%",
                                Rating = double.TryParse(profile.StarRating, out double rating) ? rating : 0.0,
                                ProfileImage = profile.ImageURL
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
                await Application.Current.MainPage.DisplayAlert("Error", "Could not load player data. Please try again later.", "OK");
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

    public class HooperViewModel : BindableObject
    {
        // Base properties
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Position { get; set; }
        public string Location { get; set; }
        public int Rank { get; set; }
        public int GamesPlayed { get; set; }
        public string Record { get; set; }
        public string WinPercentage { get; set; }
        public double Rating { get; set; }
        public string ProfileImage { get; set; }

        // Computed properties
        public string UsernameDisplay => $"@{Username}";
        public string PositionLocation => $"{Position} • {Location}";
        public string RatingDisplay => Rating.ToString("0.0");

        // Profile image handling
        public bool HasValidImage => !string.IsNullOrEmpty(ProfileImage) &&
                                     (ProfileImage.StartsWith("http://") ||
                                      ProfileImage.StartsWith("https://"));

        // Placeholder coloring
        public string Initials { get; private set; }
        public Color InitialsColor { get; private set; }

        public void InitProperties()
        {
            // Generate initials from username
            Initials = !string.IsNullOrEmpty(Username) && Username.Length > 0
                ? Username.Substring(0, Math.Min(2, Username.Length)).ToUpper()
                : "?";

            // Generate consistent color based on username
            InitialsColor = GetUsernameColor(Username);

            // Call property changed for computed properties
            OnPropertyChanged(nameof(UsernameDisplay));
            OnPropertyChanged(nameof(PositionLocation));
            OnPropertyChanged(nameof(RatingDisplay));
            OnPropertyChanged(nameof(HasValidImage));
            OnPropertyChanged(nameof(Initials));
            OnPropertyChanged(nameof(InitialsColor));
        }

        private Color GetUsernameColor(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Colors.Purple;

            // Generate hash from username for consistent color
            int hash = 0;
            foreach (char c in username)
            {
                hash = (hash * 31) + c;
            }

            // Define pleasant colors
            var colors = new List<Color>
            {
                Color.FromArgb("#512BD4"), // Purple (primary color)
                Color.FromArgb("#3498db"), // Blue
                Color.FromArgb("#2ecc71"), // Green
                Color.FromArgb("#e74c3c"), // Red
                Color.FromArgb("#f39c12"), // Orange
                Color.FromArgb("#9b59b6"), // Violet
                Color.FromArgb("#1abc9c"), // Teal
                Color.FromArgb("#34495e")  // Dark Blue
            };

            // Pick color based on hash
            return colors[Math.Abs(hash) % colors.Count];
        }
    }
}