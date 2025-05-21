using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UltimateHoopers.Converter;
using UltimateHoopers.Helpers;
using UltimateHoopers.Models;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using System.Collections.Generic;

namespace UltimateHoopers.Pages
{
    public partial class FindRunsPage : ContentPage
    {
        private FindRunsViewModel _viewModel;

        public FindRunsPage()
        {
            InitializeComponent();

            Debug.WriteLine("=== FindRunsPage Constructor Start ===");

            // Create and initialize the ViewModel
            _viewModel = new FindRunsViewModel();
            BindingContext = _viewModel;

            Debug.WriteLine($"ViewModel created and bound. BindingContext is null: {BindingContext == null}");

            // Additional setup that couldn't be done in XAML
            SetupUI();

            Debug.WriteLine("=== FindRunsPage Constructor End ===");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("=== FindRunsPage OnAppearing Start ===");

            try
            {
                // Force load data when page appears
                await _viewModel.LoadRunsAsync();
                Debug.WriteLine($"After LoadRunsAsync - Runs count: {_viewModel.Runs?.Count ?? -1}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "Could not load run data. Please try again later.", "OK");
            }

            Debug.WriteLine("=== FindRunsPage OnAppearing End ===");
        }
        private async void OnJoinButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Join button clicked");

                if (sender is Button button && button.BindingContext is Run run)
                {
                    Debug.WriteLine($"Join button clicked for run: {run.Name}");
                    await _viewModel.JoinRunCommand.ExecuteAsync(run);
                }
                else
                {
                    Debug.WriteLine("Could not get run from button binding context");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnJoinButtonClicked: {ex.Message}");
                await DisplayAlert("Error", $"Could not join run: {ex.Message}", "OK");
            }
        }

        private async void OnRunItemTapped(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Run item tapped");

                if (sender is Frame frame && frame.BindingContext is Run run)
                {
                    Debug.WriteLine($"Run item tapped: {run.Name}");
                    await _viewModel.ViewPrivateRunDetailsCommand.ExecuteAsync(run);
                }
                else
                {
                    Debug.WriteLine("Could not get run from frame binding context");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnRunItemTapped: {ex.Message}");
                await DisplayAlert("Error", $"Could not view run details: {ex.Message}", "OK");
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
        private async void OnBackClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("FindRunsPage: OnHomeClicked - using DirectNavigationHelper");
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindRunsPage: Error navigating to HomePage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnStatsNavigationClicked(object sender, TappedEventArgs e)
        {
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

    // Enhanced ViewModel with comprehensive debugging
    public class FindRunsViewModel : BindableObject
    {
        private ObservableCollection<Run> _allRuns;
        private ObservableCollection<Run> _runs;
        private bool _isLoading;
        private bool _isRefreshing;
        private string _searchText = "";
        private string _selectedDate = "All";
        private string _selectedSkillLevel = "All Levels";
        private bool _mapViewEnabled = false;

        public FindRunsViewModel()
        {
            Debug.WriteLine("=== FindRunsViewModel Constructor Start ===");

            // Initialize collections immediately
            _allRuns = new ObservableCollection<Run>();
            _runs = new ObservableCollection<Run>();

            Debug.WriteLine($"Collections initialized. _allRuns: {_allRuns != null}, _runs: {_runs != null}");

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadRunsAsync());
            JoinRunCommand = new Command<Run>(async (run) => await JoinRun(run));
            ViewPrivateRunDetailsCommand = new Command<Run>(async (run) => await ViewRunDetails(run));
            CreateRunCommand = new Command(async () => await CreateRun());
            ToggleMapViewCommand = new Command(() => MapViewEnabled = !MapViewEnabled);
            LoadMoreCommand = new Command(async () => await LoadMoreRuns());

            Debug.WriteLine("Commands initialized");

            // Load data immediately in constructor for testing
            Task.Run(async () => {
                await Task.Delay(100); // Small delay to ensure UI is ready
                await LoadRunsAsync();
            });

            Debug.WriteLine("=== FindRunsViewModel Constructor End ===");
        }

        public ObservableCollection<Run> Runs
        {
            get
            {
                Debug.WriteLine($"Runs getter called. Count: {_runs?.Count ?? -1}");
                return _runs;
            }
            private set
            {
                Debug.WriteLine($"Runs setter called. New count: {value?.Count ?? -1}, Old count: {_runs?.Count ?? -1}");
                _runs = value;
                OnPropertyChanged();
                Debug.WriteLine("Runs PropertyChanged notification sent");
            }
        }

        public bool IsNotLoading => !_isLoading;

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
                    Debug.WriteLine($"IsLoading changed to: {_isLoading}");
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
                Debug.WriteLine($"IsRefreshing changed to: {_isRefreshing}");
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterRuns();
            }
        }

        public string SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                FilterRuns();
            }
        }

        public string SelectedSkillLevel
        {
            get => _selectedSkillLevel;
            set
            {
                _selectedSkillLevel = value;
                OnPropertyChanged();
                FilterRuns();
            }
        }

        public bool MapViewEnabled
        {
            get => _mapViewEnabled;
            set
            {
                _mapViewEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MapViewToggleText));
            }
        }

        public string MapViewToggleText => MapViewEnabled ? "List View" : "Map View";

        // Commands
        public Command RefreshCommand { get; }
        public Command<Run> JoinRunCommand { get; }
        public Command<Run> ViewPrivateRunDetailsCommand { get; }
        public Command CreateRunCommand { get; }
        public Command ToggleMapViewCommand { get; }
        public Command LoadMoreCommand { get; }

        public async Task LoadRunsAsync()
        {
            Debug.WriteLine("=== LoadRunsAsync Start ===");

            try
            {
                IsLoading = true;
                IsRefreshing = true;

                Debug.WriteLine("Loading and Refreshing flags set to true");

                // Small delay for loading indicator
                await Task.Delay(100);

                // Clear existing runs
                _allRuns.Clear();
                Debug.WriteLine("_allRuns cleared");

                // Load mock data
                LoadMockRuns();
                Debug.WriteLine($"After LoadMockRuns: _allRuns count = {_allRuns.Count}");

                // Apply current filters
                FilterRuns();
                Debug.WriteLine($"After FilterRuns: Runs count = {Runs.Count}");

                // Force property change notification
                OnPropertyChanged(nameof(Runs));
                Debug.WriteLine("Forced property change notification for Runs");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in LoadRunsAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load runs: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
                Debug.WriteLine("Loading and Refreshing flags set to false");
            }

            Debug.WriteLine("=== LoadRunsAsync End ===");
        }

        private void LoadMockRuns()
        {
            Debug.WriteLine("=== LoadMockRuns Start ===");

            try
            {
                // Create mock runs with detailed logging
                var mockRuns = new List<Run>();

                var run1 = new Run
                {
                    Id = "1",
                    Name = "Downtown Pickup Game",
                    Location = "Downtown Court",
                    Address = "123 Main St, Conyers, GA",
                    Date = DateTime.Now.AddDays(2),
                    Time = "7:00 PM - 9:00 PM",
                    HostName = "Michael Jordan",
                    HostId = "user123",
                    SkillLevel = "All Levels",
                    GameType = "5-on-5",
                    IsPublic = true,
                    Description = "Weekly pickup game, open to all skill levels. Bring light and dark shirts!",
                    PlayerLimit = 10,
                    CurrentPlayerCount = 6,
                    CourtImageUrl = "https://images.unsplash.com/photo-1518626413561-907586085645?q=80&w=1000&auto=format&fit=crop",
                    Cost = 0,
                    Distance = 1.2,
                    Players = new ObservableCollection<Player>()
                };

                var run2 = new Run
                {
                    Id = "2",
                    Name = "Pro Run",
                    Location = "Elite Sports Center",
                    Address = "456 Elm St, Conyers, GA",
                    Date = DateTime.Now.AddDays(1),
                    Time = "6:00 PM - 8:00 PM",
                    HostName = "LeBron James",
                    HostId = "user456",
                    SkillLevel = "Advanced",
                    GameType = "5-on-5",
                    IsPublic = true,
                    Description = "High-level run for experienced players. Full court games with refs.",
                    PlayerLimit = 15,
                    CurrentPlayerCount = 15,
                    CourtImageUrl = "https://images.unsplash.com/photo-1505666287802-931dc83d1b52?q=80&w=1000&auto=format&fit=crop",
                    Cost = 10,
                    Distance = 3.5,
                    Players = new ObservableCollection<Player>()
                };

                var run3 = new Run
                {
                    Id = "3",
                    Name = "Morning Shootaround",
                    Location = "Community Center",
                    Address = "789 Oak Ave, Conyers, GA",
                    Date = DateTime.Now.AddDays(3),
                    Time = "8:00 AM - 10:00 AM",
                    HostName = "Steph Curry",
                    HostId = "user789",
                    SkillLevel = "Intermediate",
                    GameType = "3-on-3",
                    IsPublic = true,
                    Description = "Early morning games for those who like to start their day with basketball.",
                    PlayerLimit = 12,
                    CurrentPlayerCount = 8,
                    CourtImageUrl = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop",
                    Cost = 5,
                    Distance = 2.1,
                    Players = new ObservableCollection<Player>()
                };

                mockRuns.Add(run1);
                mockRuns.Add(run2);
                mockRuns.Add(run3);

                Debug.WriteLine($"Created {mockRuns.Count} mock runs");

                // Add each run to _allRuns with logging
                foreach (var run in mockRuns)
                {
                    // Add some mock players
                    for (int i = 0; i < Math.Min(3, run.CurrentPlayerCount); i++)
                    {
                        run.Players.Add(new Player
                        {
                            Id = $"player{i}_{run.Id}",
                            Name = $"Player {i + 1}",
                            IsHost = i == 0
                        });
                    }

                    _allRuns.Add(run);
                    Debug.WriteLine($"Added run: {run.Name} (ID: {run.Id}) with {run.Players.Count} players");
                }

                Debug.WriteLine($"Total runs in _allRuns: {_allRuns.Count}");

                // Verify each run
                for (int i = 0; i < _allRuns.Count; i++)
                {
                    var run = _allRuns[i];
                    Debug.WriteLine($"Run {i}: {run.Name} - Valid: {run != null}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in LoadMockRuns: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Debug.WriteLine("=== LoadMockRuns End ===");
        }

        private void FilterRuns()
        {
            Debug.WriteLine("=== FilterRuns Start ===");

            try
            {
                Debug.WriteLine($"_allRuns count: {_allRuns?.Count ?? -1}");
                Debug.WriteLine($"Current filters - Date: '{SelectedDate}', Skill: '{SelectedSkillLevel}', Search: '{SearchText}'");

                if (_allRuns == null || _allRuns.Count == 0)
                {
                    Debug.WriteLine("No runs to filter, creating empty collection");
                    Runs = new ObservableCollection<Run>();
                    return;
                }

                var filtered = _allRuns.AsEnumerable();
                Debug.WriteLine($"Starting with {filtered.Count()} runs");

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(r =>
                        (r.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (r.Location?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (r.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                    Debug.WriteLine($"After search filter: {filtered.Count()} runs");
                }

                // Filter by skill level
                if (SelectedSkillLevel != "All Levels")
                {
                    filtered = filtered.Where(r => r.SkillLevel == SelectedSkillLevel);
                    Debug.WriteLine($"After skill level filter: {filtered.Count()} runs");
                }

                // Filter by date
                if (SelectedDate != "All")
                {
                    var today = DateTime.Today;
                    switch (SelectedDate)
                    {
                        case "Today":
                            filtered = filtered.Where(r => r.Date.Date == today);
                            break;
                        case "Tomorrow":
                            filtered = filtered.Where(r => r.Date.Date == today.AddDays(1));
                            break;
                        case "This Weekend":
                            var saturday = today.AddDays((int)DayOfWeek.Saturday - (int)today.DayOfWeek);
                            var sunday = saturday.AddDays(1);
                            filtered = filtered.Where(r => r.Date.Date == saturday || r.Date.Date == sunday);
                            break;
                        case "Next Week":
                            var nextWeekStart = today.AddDays(7 - (int)today.DayOfWeek);
                            var nextWeekEnd = nextWeekStart.AddDays(6);
                            filtered = filtered.Where(r => r.Date.Date >= nextWeekStart && r.Date.Date <= nextWeekEnd);
                            break;
                    }
                    Debug.WriteLine($"After date filter: {filtered.Count()} runs");
                }

                var filteredList = filtered.ToList();
                Debug.WriteLine($"Final filtered count: {filteredList.Count}");

                // Log details of filtered runs
                for (int i = 0; i < filteredList.Count; i++)
                {
                    var run = filteredList[i];
                    Debug.WriteLine($"Filtered Run {i}: {run.Name} - Date: {run.Date:yyyy-MM-dd}");
                }

                // Update the Runs collection on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Debug.WriteLine("Updating Runs collection on UI thread");
                        Runs = new ObservableCollection<Run>(filteredList);
                        Debug.WriteLine($"Runs collection updated. New count: {Runs.Count}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating Runs collection: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in FilterRuns: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Debug.WriteLine("=== FilterRuns End ===");
        }

        private async Task JoinRun(Run run)
        {
            Debug.WriteLine($"JoinRun called for: {run?.Name ?? "null"}");

            if (run == null) return;

            try
            {
                if (run.IsFull)
                {
                    bool joinWaitlist = await Application.Current.MainPage.DisplayAlert(
                        "Run is Full",
                        "This run is currently full. Would you like to join the waitlist?",
                        "Join Waitlist", "Cancel");

                    if (joinWaitlist)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Waitlist",
                            "You've been added to the waitlist. We'll notify you if a spot opens up!",
                            "OK");
                    }
                    return;
                }

                if (run.Cost > 0)
                {
                    bool payNow = await Application.Current.MainPage.DisplayAlert(
                        "Payment Required",
                        $"This run requires a payment of ${run.Cost:F2}. Would you like to pay now?",
                        "Pay Now", "Cancel");

                    if (!payNow) return;
                }

                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Join Run",
                    $"Would you like to join this run at {run.Location} on {run.FormattedDate}?",
                    "Join", "Cancel");

                if (confirmed)
                {
                    run.CurrentPlayerCount++;
                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You've joined the run at {run.Location}! See you on the court!",
                        "OK");
                    OnPropertyChanged(nameof(Runs));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error joining run: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Could not join the run. Please try again later.", "OK");
            }
        }

        private async Task ViewRunDetails(Run run)
        {
            Debug.WriteLine($"ViewRunDetails called for: {run?.Name ?? "null"}");
            try
            {
                if (run == null) return;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var detailsPage = new PrivateRunDetailsPage(run);
                    await Application.Current.MainPage.Navigation.PushAsync(detailsPage);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error viewing run details: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Could not open run details. Please try again.", "OK");
            }
        }

        private async Task CreateRun()
        {
            Debug.WriteLine("CreateRun called");
            try
            {
                bool isHost = App.User?.IsHost ?? false;

                if (!isHost)
                {
                    bool upgrade = await Application.Current.MainPage.DisplayAlert(
                        "Host Account Required",
                        "Creating a run requires a Host account. Would you like to upgrade?",
                        "Upgrade Now", "Cancel");

                    if (upgrade)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Upgrade",
                            "Account upgrade coming soon!",
                            "OK");
                    }
                    return;
                }

                await Application.Current.MainPage.DisplayAlert(
                    "Create Run",
                    "Creating a new run will be available soon!",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating run: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Could not create a run. Please try again later.", "OK");
            }
        }

        

        private async Task LoadMoreRuns()
        {
            Debug.WriteLine("LoadMoreRuns called");
            try
            {
                IsLoading = true;
                await Task.Delay(1000);

                // Add more mock runs
                _allRuns.Add(new Run
                {
                    Id = "4",
                    Name = "Weekend Warriors",
                    Location = "Recreation Center",
                    Address = "555 Maple St, Conyers, GA",
                    Date = DateTime.Now.AddDays(5),
                    Time = "2:00 PM - 4:00 PM",
                    HostName = "Kyrie Irving",
                    HostId = "user555",
                    SkillLevel = "Intermediate",
                    GameType = "5-on-5",
                    IsPublic = true,
                    Description = "Weekend basketball for players with some experience.",
                    PlayerLimit = 10,
                    CurrentPlayerCount = 6,
                    CourtImageUrl = "https://images.unsplash.com/photo-1518036232006-8c9ed5097053?q=80&w=1000&auto=format&fit=crop",
                    Cost = 0,
                    Distance = 5.1,
                    Players = new ObservableCollection<Player>()
                });

                FilterRuns();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading more runs: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}