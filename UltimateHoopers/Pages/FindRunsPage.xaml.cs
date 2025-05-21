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

namespace UltimateHoopers.Pages
{
    public partial class FindRunsPage : ContentPage
    {
        private FindRunsViewModel _viewModel;

        public FindRunsPage()
        {
            InitializeComponent();

            // Create and initialize the ViewModel
            _viewModel = new FindRunsViewModel();
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
                await _viewModel.LoadRunsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading runs: {ex.Message}");
                await DisplayAlert("Error", "Could not load run data. Please try again later.", "OK");
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

    // Renamed and fixed ViewModel
    public class FindRunsViewModel : BindableObject
    {
        private ObservableCollection<Run> _allRuns = new ObservableCollection<Run>();
        private ObservableCollection<Run> _runs = new ObservableCollection<Run>();
        private bool _isLoading;
        private bool _isRefreshing;
        private string _searchText;
        private string _selectedDate = "All";
        private string _selectedSkillLevel = "All Levels";
        private bool _mapViewEnabled = false;

        public ObservableCollection<Run> Runs
        {
            get => _runs;
            private set
            {
                _runs = value;
                OnPropertyChanged();
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

        public FindRunsViewModel()
        {
            RefreshCommand = new Command(async () => await LoadRunsAsync());
            JoinRunCommand = new Command<Run>(async (run) => await JoinRun(run));
            ViewPrivateRunDetailsCommand = new Command<Run>(async (run) => await ViewRunDetails(run));
            CreateRunCommand = new Command(async () => await CreateRun());
            ToggleMapViewCommand = new Command(() => MapViewEnabled = !MapViewEnabled);
            LoadMoreCommand = new Command(async () => await LoadMoreRuns());
        }

        public async Task LoadRunsAsync()
        {
            try
            {
                IsLoading = true;
                await Task.Delay(300); // Small delay for loading indicator

                // Load mock data
                _allRuns.Clear();
                LoadMockRuns();

                // Apply current filters
                FilterRuns();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading runs: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadMockRuns()
        {
            var runs = new List<Run>
            {
                new Run
                {
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
                    Distance = 1.2
                },
                new Run
                {
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
                    Distance = 3.5
                }
            };

            foreach (var run in runs)
            {
                _allRuns.Add(run);
            }
        }

        private void FilterRuns()
        {
            if (_allRuns == null || _allRuns.Count == 0)
            {
                Runs = new ObservableCollection<Run>();
                return;
            }

            var filtered = _allRuns.AsEnumerable();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(r =>
                    r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Location.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Address.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by skill level
            if (SelectedSkillLevel != "All Levels")
            {
                filtered = filtered.Where(r => r.SkillLevel == SelectedSkillLevel);
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
            }

            Runs = new ObservableCollection<Run>(filtered.ToList());
        }

        private async Task JoinRun(Run run)
        {
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
            try
            {
                IsLoading = true;
                await Task.Delay(1000);

                // Add more mock runs
                _allRuns.Add(new Run
                {
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
                    Distance = 5.1
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