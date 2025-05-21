using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using UltimateHoopers.Models;
using UltimateHoopers.Services;
using System.Linq;

namespace UltimateHoopers.ViewModels
{
    public class RunViewModel : BindableObject
    {
        private bool _isRefreshing;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _selectedDate = "All";
        private string _selectedSkillLevel = "All Levels";
        private bool _mapViewEnabled;

        // Collection of runs
        public ObservableCollection<Run> Runs { get; } = new ObservableCollection<Run>();

        // Filter properties
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
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
                // Switch between map and list view
                MapViewToggleText = _mapViewEnabled ? "Show List View" : "Show Map View";
            }
        }

        public string MapViewToggleText { get; private set; } = "Show Map View";

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand ToggleMapViewCommand { get; }
        public ICommand JoinRunCommand { get; }
        public ICommand CreateRunCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand ViewRunDetailsCommand { get; }

        // Constructor
        public RunViewModel()
        {
            // Initialize commands
            RefreshCommand = new Command(async () => await RefreshRuns());
            ToggleMapViewCommand = new Command(() => MapViewEnabled = !MapViewEnabled);
            JoinRunCommand = new Command<Run>(async (run) => await JoinRun(run));
            CreateRunCommand = new Command(async () => await CreateNewRun());
            LoadMoreCommand = new Command(async () => await LoadMoreRuns());
            ViewRunDetailsCommand = new Command<Run>(async (run) => await ViewRunDetails(run));

            // Load data
            LoadSampleData();
        }

        // Methods
        public async Task RefreshRuns()
        {
            if (IsRefreshing || IsLoading)
                return;

            try
            {
                IsRefreshing = true;
                IsLoading = true;

                // Clear existing runs
                Runs.Clear();

                // In a real app, this would call a service to get runs
                // await LoadRunsFromService();

                // For demo, load sample data
                LoadSampleData();

                // Apply filters
                FilterRuns();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing runs: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
                IsLoading = false;
            }
        }

        public void FilterRuns()
        {
            // In a real app, this would apply filters to the data
            // For now, we'll just log what would be filtered
            Debug.WriteLine($"Filtering runs: Search='{SearchText}', Date='{SelectedDate}', Skill='{SelectedSkillLevel}'");

            // Notify that the runs collection has changed
            OnPropertyChanged(nameof(Runs));
        }

        private async Task JoinRun(Run run)
        {
            if (run == null)
                return;

            try
            {
                // Check if run is full
                if (run.IsFull)
                {
                    bool joinWaitlist = await Application.Current.MainPage.DisplayAlert(
                        "Run is Full",
                        $"This run already has {run.PlayerLimit} players. Would you like to join the waitlist?",
                        "Join Waitlist", "Cancel");

                    if (joinWaitlist)
                    {
                        // Add to waitlist logic would go here
                        await Application.Current.MainPage.DisplayAlert(
                            "Waitlist",
                            "You've been added to the waitlist! We'll notify you if a spot opens up.",
                            "OK");
                    }

                    return;
                }

                // Confirm join
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Join Run",
                    $"Would you like to join '{run.Name}' at {run.Location} on {run.FormattedDate}?",
                    "Join", "Cancel");

                if (confirmed)
                {
                    // In a real app, this would call a service to join the run
                    // await _runService.JoinRunAsync(run.Id);

                    // For demo, just increment player count
                    run.CurrentPlayerCount++;

                    // Update UI
                    int index = Runs.IndexOf(run);
                    if (index >= 0)
                    {
                        Runs[index] = run;
                    }

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You've joined '{run.Name}'! See you on the court!",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error joining run: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "There was a problem joining this run. Please try again.",
                    "OK");
            }
        }

        private async Task CreateNewRun()
        {
            // In a real app, this would check if the user is a host
            bool isHost = App.User?.IsHost ?? false;

            if (!isHost)
            {
                bool upgrade = await Application.Current.MainPage.DisplayAlert(
                    "Host Account Required",
                    "You need a Host account to create runs. Host accounts cost $9.99/month. Would you like to upgrade your account?",
                    "Upgrade", "Cancel");

                if (upgrade)
                {
                    // Navigate to upgrade page
                    await Application.Current.MainPage.DisplayAlert(
                        "Upgrade",
                        "Account upgrade feature coming soon!",
                        "OK");
                }

                return;
            }

            // Navigate to create run page
            // In a real app, this would navigate to the CreateRunPage
            await Application.Current.MainPage.DisplayAlert(
                "Create Run",
                "Navigate to CreateRunPage",
                "OK");
        }

        private async Task LoadMoreRuns()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;

                // In a real app, this would load more runs from a service
                // For demo, just add a few more sample runs
                var moreRuns = CreateSampleRuns(3, Runs.Count);

                foreach (var run in moreRuns)
                {
                    Runs.Add(run);
                }

                await Application.Current.MainPage.DisplayAlert(
                    "Loaded More Runs",
                    $"Added {moreRuns.Count} more runs",
                    "OK");
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

        private async Task ViewRunDetails(Run run)
        {
            if (run == null)
                return;

            // In a real app, this would navigate to the run details page
            await Application.Current.MainPage.DisplayAlert(
                "Run Details",
                $"Viewing details for: {run.Name}",
                "OK");
        }

        private void LoadSampleData()
        {
            var sampleRuns = CreateSampleRuns(5);

            foreach (var run in sampleRuns)
            {
                Runs.Add(run);
            }
        }

        private List<Run> CreateSampleRuns(int count, int startIndex = 0)
        {
            var runs = new List<Run>();
            var random = new Random();
            var locations = new string[] { "Downtown Court", "Westside Rec Center", "Eastside Community Center", "University Gym", "Central Park Courts" };
            var addresses = new string[] { "123 Main St", "456 Park Ave", "789 East St", "101 College Blvd", "202 Central Park" };
            var hosts = new string[] { "Michael Johnson", "Sarah Thompson", "Kevin Brown", "Lisa Williams", "David Chen" };
            var skillLevels = new string[] { "All Levels", "Beginner", "Intermediate", "Advanced" };

            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                int locationIndex = index % locations.Length;

                var run = new Run
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = locations[locationIndex],
                    Location = locations[locationIndex],
                    Address = $"{addresses[locationIndex]}, Atlanta, GA",
                    Date = DateTime.Now.AddDays(random.Next(1, 8)),
                    Time = $"{random.Next(6, 10)}:{(random.Next(0, 2) == 0 ? "00" : "30")} PM",
                    HostName = hosts[locationIndex % hosts.Length],
                    HostId = $"host{locationIndex}",
                    SkillLevel = skillLevels[random.Next(0, skillLevels.Length)],
                    PlayerLimit = random.Next(1, 3) == 1 ? 10 : 8, // 5v5 or 4v4
                    CurrentPlayerCount = random.Next(2, 8),
                    Description = $"Pickup game at {locations[locationIndex]}. All welcome!",
                    CourtImageUrl = "https://placehold.co/600x400/333333/FFFFFF?text=Court+Image",
                    Distance = Math.Round(random.NextDouble() * 3 + 0.5, 1) // 0.5 to 3.5 miles
                };

                runs.Add(run);
            }

            return runs;
        }
    }
}