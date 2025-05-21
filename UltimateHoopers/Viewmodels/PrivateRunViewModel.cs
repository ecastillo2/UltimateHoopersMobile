using Domain;
using Domain.DtoModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateHoopers.Models;
using UltimateHoopers.Pages;
using UltimateHoopers.Services;

namespace UltimateHoopers.ViewModels
{
    public class PrivateRunViewModel : BindableObject
    {
        // Base properties
        public string? PrivateRunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }

        public decimal? Cost { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Description { get; set; }
        public string? RunTime { get; set; }
        public string? EndTime { get; set; }
        public string? Type { get; set; }
        public string? CreatedDate { get; set; }
        public string? PrivateRunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public int? PlayerLimit { get; set; }
        public string? Username { get; set; }  // Add the missing Username property

        private bool _isRefreshing;
        private bool _isLoading;
        private string _searchText;
        private string _selectedDate;
        private string _selectedSkillLevel;
        private bool _mapViewEnabled;

        // Properties for binding
        public ObservableCollection<Run> Runs { get; } = new ObservableCollection<Run>();

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
                OnPropertyChanged(nameof(MapViewToggleText));
            }
        }

        public string MapViewToggleText => MapViewEnabled ? "List View" : "Map View";

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand JoinRunCommand { get; }
        public ICommand ViewRunDetailsCommand { get; }
        public ICommand CreateRunCommand { get; }
        public ICommand ToggleMapViewCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand SearchCommand { get; }

        // Constructor
        public PrivateRunViewModel()
        {
            // Initialize commands
            RefreshCommand = new Command(async () => await LoadRunsAsync());
            JoinRunCommand = new Command<Run>(async (run) => await JoinRun(run));
            ViewRunDetailsCommand = new Command<Run>(async (run) => await ViewRunDetails(run));
            CreateRunCommand = new Command(async () => await CreateRun());
            ToggleMapViewCommand = new Command(() => MapViewEnabled = !MapViewEnabled);
            LoadMoreCommand = new Command(async () => await LoadMoreRuns());
            SearchCommand = new Command<string>((searchText) => SearchText = searchText);

            // Set default values
            _selectedDate = "All";
            _selectedSkillLevel = "All Levels";
            _mapViewEnabled = false;

            // Load initial data
            LoadMockData();
        }

        // Methods
        private async Task LoadRunsAsync()
        {
            if (IsRefreshing || IsLoading)
                return;

            try
            {
                IsRefreshing = true;
                IsLoading = true;
                Debug.WriteLine("Loading runs...");

                // In a real app, this would call a service to get runs
                // For now, just simulate a network delay
                await Task.Delay(1000);

                // Clear current runs and load mock data
                Runs.Clear();
                LoadMockData();

                Debug.WriteLine($"Loaded {Runs.Count} runs");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading runs: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Could not load runs. Please try again later.", "OK");
                });
            }
            finally
            {
                IsRefreshing = false;
                IsLoading = false;
            }
        }

        private void LoadMockData()
        {
            // Create mock run data
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
                },
                new Run
                {
                    Name = "Sunday Morning Hoops",
                    Location = "Community Center",
                    Address = "789 Oak St, Conyers, GA",
                    Date = DateTime.Now.AddDays(4),
                    Time = "10:00 AM - 12:00 PM",
                    HostName = "Steph Curry",
                    HostId = "user789",
                    SkillLevel = "Intermediate",
                    GameType = "3-on-3",
                    IsPublic = true,
                    Description = "Sunday morning 3-on-3 games. Coffee and donuts provided!",
                    PlayerLimit = 12,
                    CurrentPlayerCount = 9,
                    CourtImageUrl = "https://images.unsplash.com/photo-1469515634827-fca11a804eca?q=80&w=1000&auto=format&fit=crop",
                    Cost = 5,
                    Distance = 2.8
                },
                new Run
                {
                    Name = "Private Squad Run",
                    Location = "The Arena",
                    Address = "321 Pine St, Conyers, GA",
                    Date = DateTime.Now.AddDays(3),
                    Time = "8:00 PM - 10:00 PM",
                    HostName = "Kevin Durant",
                    HostId = "user321",
                    SkillLevel = "Advanced",
                    GameType = "5-on-5",
                    IsPublic = false,
                    Description = "Private run for selected players only. High intensity games.",
                    PlayerLimit = 10,
                    CurrentPlayerCount = 8,
                    CourtImageUrl = "https://images.unsplash.com/photo-1518079304312-4bb6a224b40a?q=80&w=1000&auto=format&fit=crop",
                    Cost = 0,
                    Distance = 4.2
                }
            };

            foreach (var run in runs)
            {
                Runs.Add(run);
            }
        }

        private void FilterRuns()
        {
            // In a real app, this would filter the runs based on search text, selected date, and skill level
            // For now, just log the filter criteria
            Debug.WriteLine($"Filtering runs - Search: {SearchText}, Date: {SelectedDate}, Skill Level: {SelectedSkillLevel}");
        }

        private async Task LoadMoreRuns()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine("Loading more runs...");

                // Simulate network delay
                await Task.Delay(1000);

                // Add a couple more mock runs
                Runs.Add(new Run
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
                    Description = "Weekend basketball for players with some experience. Games to 15, win by 2.",
                    PlayerLimit = 10,
                    CurrentPlayerCount = 6,
                    CourtImageUrl = "https://images.unsplash.com/photo-1518036232006-8c9ed5097053?q=80&w=1000&auto=format&fit=crop",
                    Cost = 0,
                    Distance = 5.1
                });

                Runs.Add(new Run
                {
                    Name = "Ladies Only Run",
                    Location = "Women's Sports Complex",
                    Address = "777 Birch St, Conyers, GA",
                    Date = DateTime.Now.AddDays(7),
                    Time = "6:30 PM - 8:30 PM",
                    HostName = "Candace Parker",
                    HostId = "user777",
                    SkillLevel = "All Levels",
                    GameType = "5-on-5",
                    IsPublic = true,
                    Description = "Women's basketball night. All skill levels welcome.",
                    PlayerLimit = 15,
                    CurrentPlayerCount = 10,
                    CourtImageUrl = "https://images.unsplash.com/photo-1517747614396-d21a78b850e8?q=80&w=1000&auto=format&fit=crop",
                    Cost = 5,
                    Distance = 6.3
                });

                Debug.WriteLine($"Added more runs. Total count: {Runs.Count}");
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

        private async Task JoinRun(Run run)
        {
            try
            {
                if (run == null)
                    return;

                // Check if the run is full
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

                // Check if the run has a cost
                if (run.Cost > 0)
                {
                    bool payNow = await Application.Current.MainPage.DisplayAlert(
                        "Payment Required",
                        $"This run requires a payment of ${run.Cost:F2}. Would you like to pay now?",
                        "Pay Now", "Cancel");

                    if (payNow)
                    {
                        // Navigate to payment page or process payment
                        await Application.Current.MainPage.DisplayAlert(
                            "Payment",
                            "Payment processing coming soon!",
                            "OK");
                    }
                    else
                    {
                        return;
                    }
                }

                // Confirm join
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Join Run",
                    $"Would you like to join this run at {run.Location} on {run.FormattedDate}?",
                    "Join", "Cancel");

                if (confirmed)
                {
                    // In a real app, this would call a service to join the run
                    // For now, just increment the player count and show a success message
                    run.CurrentPlayerCount++;

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"You've joined the run at {run.Location}! See you on the court!",
                        "OK");

                    // Update UI
                    OnPropertyChanged(nameof(Runs));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error joining run: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Could not join the run. Please try again later.", "OK");
                });
            }
        }

        private async Task ViewRunDetails(Run run)
        {
            try
            {
                if (run == null)
                    return;

                // Navigate to run details page
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var detailsPage = new PrivateRunDetailsPage(run);
                    await Application.Current.MainPage.Navigation.PushAsync(detailsPage);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error viewing run details: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Could not open run details. Please try again.", "OK");
                });
            }
        }

        private async Task CreateRun()
        {
            try
            {
                // Check if the user has host privileges
                bool isHost = App.User?.IsHost ?? false;

                if (!isHost)
                {
                    // Show upgrade dialog
                    bool upgrade = await Application.Current.MainPage.DisplayAlert(
                        "Host Account Required",
                        "Creating a run requires a Host account. Would you like to upgrade?",
                        "Upgrade Now", "Cancel");

                    if (upgrade)
                    {
                        // Navigate to upgrade page or show upgrade options
                        await Application.Current.MainPage.DisplayAlert(
                            "Upgrade",
                            "Account upgrade coming soon!",
                            "OK");
                    }

                    return;
                }

                // In a real app, this would navigate to a create run page
                await Application.Current.MainPage.DisplayAlert(
                    "Create Run",
                    "Creating a new run will be available soon!",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating run: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Could not create a run. Please try again later.", "OK");
                });
            }
        }
    }
}