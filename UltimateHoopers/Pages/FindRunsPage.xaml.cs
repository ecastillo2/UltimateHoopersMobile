using Domain;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UltimateHoopers.Converter;
using UltimateHoopers.Helpers;
using UltimateHoopers.Models;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using ControlsAnimation = Microsoft.Maui.Controls.Animation;
using MauiAnimation = Microsoft.Maui.Animations.Animation;

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
                // Update Create Run button visibility based on user account type
                UpdateCreateRunButtonVisibility();

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

        // FIXED: Updated OnJoinButtonClicked with confirmation dialog and proper Run object retrieval
        private async void OnJoinButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Join button clicked");

                RunDto run = null;

                // Try multiple ways to get the Run object
                if (sender is Button button)
                {
                    // Method 1: Check CommandParameter
                    if (button.CommandParameter is RunDto commandRun)
                    {
                        run = commandRun;
                        Debug.WriteLine($"Got run from CommandParameter: {run.Name}");
                    }
                    // Method 2: Check BindingContext
                    else if (button.BindingContext is RunDto contextRun)
                    {
                        run = contextRun;
                        Debug.WriteLine($"Got run from BindingContext: {run.Name}");
                    }
                    // Method 3: Walk up the visual tree to find the Run
                    else
                    {
                        run = FindRunInParentContext(button);
                        if (run != null)
                        {
                            Debug.WriteLine($"Got run from parent context: {run.Name}");
                        }
                    }
                }

                if (run != null)
                {
                    // Show confirmation dialog with run details
                    string message = $"Are you sure you want to join the run at {run.Location}?";

                    if (run.Cost > 0)
                    {
                        message += $"\n\nCost: ${run.Cost:F2} per player";
                    }

                    if (!string.IsNullOrEmpty(run.Time))
                    {
                        message += $"\nTime: {run.Time}";
                    }

                    if (!string.IsNullOrEmpty(run.FormattedDate))
                    {
                        message += $"\nDate: {run.FormattedDate}";
                    }

                    bool userConfirmed = await DisplayAlert(
                        "Join Run",
                        message,
                        "Yes, Join",
                        "Cancel");

                    if (userConfirmed)
                    {
                        Debug.WriteLine($"User confirmed joining run: {run.Name}");

                        if (_viewModel?.JoinRunCommand?.CanExecute(run) == true)
                        {
                            _viewModel.JoinRunCommand.Execute(run);
                        }
                        else
                        {
                            Debug.WriteLine("JoinRunCommand is not available or cannot execute");

                            // Fallback: Handle join manually
                            await HandleJoinRunFallback(run);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("User cancelled joining the run");
                    }
                }
                else
                {
                    Debug.WriteLine("Could not get run from any source");
                    await DisplayAlert("Error", "Could not identify the run to join", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnJoinButtonClicked: {ex.Message}");
                await DisplayAlert("Error", $"Could not join run: {ex.Message}", "OK");
            }
        }

        // Helper method to find Run in parent context
        private RunDto FindRunInParentContext(Element element)
        {
            try
            {
                var parent = element.Parent;
                while (parent != null)
                {
                    if (parent.BindingContext is RunDto run)
                    {
                        Debug.WriteLine($"Found Run in parent: {parent.GetType().Name}");
                        return run;
                    }
                    parent = parent.Parent;
                }

                Debug.WriteLine("No Run found in parent hierarchy");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding run in parent context: {ex.Message}");
                return null;
            }
        }

        // Fallback method to handle joining manually if command fails
        private async Task HandleJoinRunFallback(RunDto run)
        {
            try
            {
                if (run.IsFull)
                {
                    bool joinWaitlist = await DisplayAlert(
                        "Run is Full",
                        "This run is currently full. Would you like to join the waitlist?",
                        "Join Waitlist", "Cancel");

                    if (joinWaitlist)
                    {
                        await DisplayAlert(
                            "Waitlist",
                            "You've been added to the waitlist. We'll notify you if a spot opens up!",
                            "OK");
                    }
                    return;
                }

                if (run.Cost > 0)
                {
                    bool payNow = await DisplayAlert(
                        "Payment Required",
                        $"This run requires a payment of ${run.Cost:F2}. Would you like to pay now?",
                        "Pay Now", "Cancel");

                    if (!payNow) return;

                    // In a real app, you would process payment here
                    await DisplayAlert("Payment", "Payment processing coming soon!", "OK");
                }

                // Add user to run (simulate joining)
                run.CurrentPlayerCount++;

                await DisplayAlert(
                    "Success",
                    $"You've joined the run at {run.Location}! See you on the court!",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in fallback join handling: {ex.Message}");
                await DisplayAlert("Error", "Could not complete join request", "OK");
            }
        }

        private async void OnRunItemTapped(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Run item tapped");

                if (sender is Frame frame && frame.BindingContext is RunDto run)
                {
                    Debug.WriteLine($"Run item tapped: {run.Name}");

                    // Navigate to run details page
                    await Navigation.PushAsync(new RunDetailsPage(run));
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
        private ObservableCollection<RunDto> _allRuns;
        private ObservableCollection<RunDto> _runs;
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
            _allRuns = new ObservableCollection<RunDto>();
            _runs = new ObservableCollection<RunDto>();

            Debug.WriteLine($"Collections initialized. _allRuns: {_allRuns != null}, _runs: {_runs != null}");

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadRunsAsync());
            JoinRunCommand = new Command<RunDto>(async (run) => await JoinRun(run));
            ViewPrivateRunDetailsCommand = new Command<RunDto>(async (run) => await ViewRunDetails(run));
            CreateRunCommand = new Command(async () => await CreateRun());
            ToggleMapViewCommand = new Command(() => MapViewEnabled = !MapViewEnabled);
            LoadMoreCommand = new Command(async () => await LoadMoreRuns());

            Debug.WriteLine("Commands initialized");
            Debug.WriteLine("=== FindRunsViewModel Constructor End ===");
        }

        public ObservableCollection<RunDto> Runs
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
        public Command<RunDto> JoinRunCommand { get; }
        public Command<RunDto> ViewPrivateRunDetailsCommand { get; }
        public Command CreateRunCommand { get; }
        public Command ToggleMapViewCommand { get; }
        public Command LoadMoreCommand { get; }

        public async Task LoadRunsAsync()
        {
            try
            {
                IsLoading = true;
                await Task.Delay(300);

                Debug.WriteLine("=== LoadRunsAsync Start ===");
                _allRuns.Clear();

                bool dataLoaded = false;

                try
                {
                    var serviceProvider = MauiProgram.CreateMauiApp().Services;
                    var privateRunService = serviceProvider.GetService<IRunService>();

                    if (privateRunService == null)
                    {
                        privateRunService = new RunService();
                    }

                    Debug.WriteLine("Attempting to load runs from service...");

                    var privateRuns = await privateRunService.GetRunsAsync();

                    if (privateRuns != null && privateRuns.Count > 0)
                    {
                        Debug.WriteLine($"Loaded {privateRuns.Count} private runs from service");

                        foreach (var privateRun in privateRuns)
                        {
                            try
                            {
                                var run = ConvertRunToRun(privateRun);
                                _allRuns.Add(run);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error converting private run to run model: {ex.Message}");
                            }
                        }

                        dataLoaded = true;
                        Debug.WriteLine($"Successfully converted {_allRuns.Count} runs");
                    }
                    else
                    {
                        Debug.WriteLine("No private runs returned from service");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading runs from service: {ex.Message}");
                }

                if (!dataLoaded)
                {
                    Debug.WriteLine("Loading mock data...");
                    LoadMockRuns();
                    dataLoaded = true;
                }

                FilterRuns();
                Debug.WriteLine($"=== LoadRunsAsync Complete - Total runs: {_allRuns.Count} ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadRunsAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                LoadMockRuns();
                FilterRuns();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private RunDto ConvertRunToRun(Domain.Run privateRun)
        {
            var run = new RunDto
            {
                Id = privateRun.RunId ?? Guid.NewGuid().ToString(),
                Name = privateRun.Name ?? "Basketball Run",
                Location = privateRun.Name ?? "Court",
                Address = $"{privateRun.Court.Address ?? ""}, {privateRun.Court.City ?? ""}, {privateRun.Court.State ?? ""}, {privateRun.Court.Zip ?? ""}".Trim(',', ' '),
                Date = privateRun.RunDate ?? DateTime.Now.AddDays(1),
                Time = $"{privateRun.RunTime ?? "6:00 PM"} - {privateRun.EndTime ?? "8:00 PM"}",
                HostName = "Host",
                HostId = privateRun.ProfileId ?? "",
                SkillLevel = privateRun.SkillLevel ?? "All Levels",
                GameType = privateRun.TeamType ?? "5-on-5",
                IsPublic = privateRun.Type?.ToLower() != "private",
                Description = privateRun.Description ?? "Come play basketball!",
                PlayerLimit = privateRun.PlayerLimit ?? 10,
                CurrentPlayerCount = new Random().Next(3, (privateRun.PlayerLimit ?? 10) - 2),
                CourtImageUrl = privateRun.ImageUrl ?? "",
                Cost = privateRun.Cost ?? 0,
                Distance = Math.Round(new Random().NextDouble() * 5 + 0.5, 1),
                Players = new ObservableCollection<Player>()
            };

            return run;
        }

        private void LoadMockRuns()
        {
            Debug.WriteLine("=== LoadMockRuns Start ===");

            try
            {
                var mockRuns = new List<RunDto>();

                var run1 = new RunDto
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

                var run2 = new RunDto
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

                var run3 = new RunDto
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

                foreach (var run in mockRuns)
                {
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
                    Runs = new ObservableCollection<RunDto>();
                    return;
                }

                var filtered = _allRuns.AsEnumerable();
                Debug.WriteLine($"Starting with {filtered.Count()} runs");

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(r =>
                        (r.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (r.Location?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (r.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                    Debug.WriteLine($"After search filter: {filtered.Count()} runs");
                }

                if (SelectedSkillLevel != "All Levels")
                {
                    filtered = filtered.Where(r => r.SkillLevel == SelectedSkillLevel);
                    Debug.WriteLine($"After skill level filter: {filtered.Count()} runs");
                }

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

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Debug.WriteLine("Updating Runs collection on UI thread");
                        Runs = new ObservableCollection<RunDto>(filteredList);
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

        private async Task JoinRun(RunDto run)
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

        private async Task ViewRunDetails(RunDto run)
        {
            Debug.WriteLine($"ViewRunDetails called for: {run?.Name ?? "null"}");
            try
            {
                if (run == null) return;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var detailsPage = new RunDetailsPage(run);
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
                        "Creating a run requires a Host account ($9.99/month). Would you like to upgrade or continue anyway?",
                        "Upgrade Now", "Continue");

                    if (upgrade)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Account Upgrade",
                            "Account upgrade feature coming soon! For now, you can still create a run.",
                            "OK");
                    }

                    // Allow them to continue regardless of upgrade choice for demo purposes
                }

                // Navigate to CreateRunPage
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Try multiple navigation approaches for reliability
                        if (Application.Current.MainPage is Shell shell)
                        {
                            await shell.Navigation.PushAsync(new CreateRunPage());
                        }
                        else if (Application.Current.MainPage.Navigation != null)
                        {
                            await Application.Current.MainPage.Navigation.PushAsync(new CreateRunPage());
                        }
                        else
                        {
                            // Fallback: show error
                            await Application.Current.MainPage.DisplayAlert(
                                "Navigation Error",
                                "Could not navigate to create run page. Please try again.",
                                "OK");
                        }
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"Navigation error: {navEx.Message}");
                        await Application.Current.MainPage.DisplayAlert(
                            "Navigation Error",
                            "Could not navigate to create run page. Please try again.",
                            "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateRun: {ex.Message}");
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

                _allRuns.Add(new RunDto
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