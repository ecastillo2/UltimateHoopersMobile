using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UltimateHoopers.Converter;
using UltimateHoopers.Helpers;
using UltimateHoopers.Models;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class FindRunsPage : ContentPage
    {
        private PrivateRunsViewModel _viewModel;

        public FindRunsPage()
        {
            InitializeComponent();

            // Create and initialize the ViewModel
            _viewModel = new PrivateRunsViewModel();
            BindingContext = _viewModel;
            // Load data when page appears
            Loaded += OnPageLoaded;

            // Additional setup that couldn't be done in XAML
            SetupUI();
        }

     

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.LoadPrivateRunsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hoopers: {ex.Message}");
                await DisplayAlert("Error", "Could not load player data. Please try again later.", "OK");
            }
        }

        private void SetupUI()
        {
            // Set default picker values if not already set
            if (DatePicker.SelectedItem == null && DatePicker.Items.Count > 0)
            {
                DatePicker.SelectedIndex = 0;
            }

            if (SkillLevelPicker.SelectedItem == null && SkillLevelPicker.Items.Count > 0)
            {
                SkillLevelPicker.SelectedIndex = 0;
            }

            // Register boolean converter if needed
            if (!Resources.TryGetValue("InvertBoolConverter", out _))
            {
                Resources.Add("InvertBoolConverter", new InvertBoolConverter());
            }
        }
       
        // Update the Create Run button visibility based on account type
        private void UpdateCreateRunButtonVisibility()
        {
            try
            {
                // Check if user is authenticated and has a Host account
                bool isHost = App.User?.IsHost ?? false;

                // Find the Create Run button by name
                if (CreateRunButton != null)
                {
                    // Update button based on account type
                    if (!isHost)
                    {
                        CreateRunButton.Text = "Upgrade to Host";
                    }
                    else
                    {
                        CreateRunButton.Text = "Create Run";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating create run button visibility: {ex.Message}");
            }
        }

        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("FindRunsPage: OnHomeClicked - using DirectNavigationHelper");

                // Use the simpler, more direct navigation helper
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindRunsPage: Error navigating to HomePage: {ex.Message}");

                // Show an error message to the user
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnStatsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to the Stats page
            try
            {
                await Shell.Current.GoToAsync("//StatsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to StatsPage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to stats page", "OK");
            }
        }

        private async void OnProfileNavigationClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Profile", "Profile page coming soon!", "OK");
        }
    }

    public class PrivateRunsViewModel : BindableObject
    {
        private ObservableCollection<PrivateRunViewModel> _allHoopers = new ObservableCollection<PrivateRunViewModel>();
        private ObservableCollection<PrivateRunViewModel> _filteredHoopers = new ObservableCollection<PrivateRunViewModel>();
        private bool _isLoading;

        public ObservableCollection<PrivateRunViewModel> FilteredHoopers
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

        public async Task LoadPrivateRunsAsync()
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
                            var hooper = new PrivateRunViewModel
                            {
                                //Username = profile.UserName ?? "",
                                //DisplayName = profile.UserName ?? "Unknown Player",
                                //Position = profile.Position ?? "Unknown",
                                //Location = profile.City ?? "Unknown Location",
                                //Rank = int.TryParse(profile.Ranking, out int rank) ? rank : 99,
                                //GamesPlayed = int.TryParse(profile.GameStatistics.TotalGames.ToString(), out int games) ? games : 0,
                                //Record = $"{profile.GameStatistics.TotalWins.ToString() ?? "0"}-{profile.GameStatistics.TotalLosses.ToString() ?? "0"}",
                                //WinPercentage = profile.GameStatistics.WinPercentage.ToString() ?? "0%",
                                //Rating = double.TryParse(profile.StarRating, out double rating) ? rating : 0.0,
                                //ProfileImage = profile.ImageURL,
                                //StyleOfPlay = profile.ScoutingReport != null ? profile.ScoutingReport.PlayStyle ?? "Unknown Player" : "Unknown Player",
                            };

                            // Initialize computed properties
                            //hooper.InitProperties();

                            _allHoopers.Add(hooper);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error converting profile to hooper model: {ex.Message}");
                        }
                    }
                }

                // Initialize filtered hoopers with all hoopers
                FilteredHoopers = new ObservableCollection<PrivateRunViewModel>(_allHoopers);
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
                FilteredHoopers = new ObservableCollection<PrivateRunViewModel>(_allHoopers);
                return;
            }

            // Remove @ symbol if present
            searchText = searchText.TrimStart('@').ToLower();

            // Filter by username
            var filtered = _allHoopers.Where(h => h.Username.ToLower().Contains(searchText)).ToList();
            FilteredHoopers = new ObservableCollection<PrivateRunViewModel>(filtered);
        }

       

        public void FilterByLocation(string location)
        {
            if (_allHoopers == null || _allHoopers.Count == 0)
                return;

            var filtered = _allHoopers.Where(h => h.Location.Contains(location)).ToList();
            FilteredHoopers = new ObservableCollection<PrivateRunViewModel>(filtered);
        }
    }

}