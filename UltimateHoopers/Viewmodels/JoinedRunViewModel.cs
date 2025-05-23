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

    namespace UltimateHoopers.ViewModels
    {
        /// <summary>
        /// ViewModel for individual Private Run items - represents a single run
        /// </summary>
        public class JoinedRunViewModel : BindableObject
        {
            // Base properties from Domain.PrivateRun
            public string? RunId { get; set; }
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
            public string? RunNumber { get; set; }
            public string? SkillLevel { get; set; }
            public string? PaymentMethod { get; set; }
            public string? TeamType { get; set; }
            public int? PlayerLimit { get; set; }
            public string? Username { get; set; }
        

            // Additional UI properties
            public string? CourtImageUrl { get; set; }
            public double Distance { get; set; }
            public int CurrentPlayerCount { get; set; }
            public string? HostName { get; set; }
            public string? GameType { get; set; }
            public bool IsPublic { get; set; }

            // Computed properties
            public string FormattedDate => RunDate?.ToString("dddd, MMMM dd, yyyy") ?? "TBD";
            public string DayOfMonth => RunDate?.Day.ToString() ?? "?";
            public string Month => RunDate?.ToString("MMM").ToUpper() ?? "TBD";
            public string PlayerCountDisplay => $"{CurrentPlayerCount}/{PlayerLimit ?? 0} players";
            public string DistanceText => $"{Distance:F1} miles away";
            public bool IsFull => CurrentPlayerCount >= (PlayerLimit ?? 0);
            public string CostText => Cost > 0 ? $"${Cost:F2}" : "Free";
            public string PrivacyText => IsPublic ? "Public" : "Private";
            public string Time => $"{RunTime} - {EndTime}";

            /// <summary>
            /// Converts this PrivateRunViewModel to a Run model for navigation
            /// </summary>
            /// <returns>Run object that can be used with PrivateRunDetailsPage</returns>
            public JoinedRunDto ToJoinedRunModel()
            {
                var run = new JoinedRunDto
                {
                    Id = RunId ?? Guid.NewGuid().ToString(),
                    Name = Name ?? "Basketball Run",
                    Location = Name ?? "Court",
                    Address = Address ?? "Location TBD",
                    Date = RunDate ?? DateTime.Now.AddDays(1),
                    Time = Time,
                    HostName = HostName ?? "Host",
                    HostId = ProfileId ?? "",
                    SkillLevel = SkillLevel ?? "All Levels",
                    GameType = GameType ?? TeamType ?? "5-on-5",
                    IsPublic = IsPublic,
                    Description = Description ?? "",
                    PlayerLimit = PlayerLimit ?? 10,
                    CurrentPlayerCount = CurrentPlayerCount,
                    CourtImageUrl = CourtImageUrl ?? "",
                    Cost = Cost ?? 0,
                    Distance = Distance
                };

               

                return run;
            }
        }

        /// <summary>
        /// ViewModel for the Private Run collection/list page
        /// Implements proper MVVM pattern with commands and data loading
        /// </summary>
        public class JoinedRunsViewModel : BindableObject
        {
            private readonly IJoinedRunService _runService;
            private ObservableCollection<JoinedRunViewModel> _runs = new ObservableCollection<JoinedRunViewModel>();
            private ObservableCollection<JoinedRunViewModel> _filteredRuns = new ObservableCollection<JoinedRunViewModel>();
            private bool _isLoading;
            private bool _isRefreshing;
            private string _searchText = string.Empty;

            public JoinedRunsViewModel(IJoinedRunService privateRunService = null)
            {
                _runService = privateRunService;
                InitializeCommands();
            }

            public ObservableCollection<JoinedRunViewModel> Runs
            {
                get => _filteredRuns;
                private set
                {
                    _filteredRuns = value;
                    OnPropertyChanged();
                }
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

            public bool IsRefreshing
            {
                get => _isRefreshing;
                set
                {
                    if (_isRefreshing != value)
                    {
                        _isRefreshing = value;
                        OnPropertyChanged();
                    }
                }
            }

            public bool IsNotLoading => !_isLoading;

            public string SearchText
            {
                get => _searchText;
                set
                {
                    if (_searchText != value)
                    {
                        _searchText = value;
                        OnPropertyChanged();
                        FilterRuns(value);
                    }
                }
            }

            // Commands
            public ICommand LoadJoinedRunsCommand { get; private set; }
            public ICommand RefreshCommand { get; private set; }
            public ICommand JoinedRunCommand { get; private set; }
            public ICommand ViewJoinedRunDetailsCommand { get; private set; }
            public ICommand SearchCommand { get; private set; }

            private void InitializeCommands()
            {
                LoadJoinedRunsCommand = new Command(async () => await LoadJoinedRunsAsync());
                RefreshCommand = new Command(async () => await RefreshRunsAsync());
                JoinedRunCommand = new Command<JoinedRunDto>(async (run) => await JoinedRunAsync(run));
                ViewJoinedRunDetailsCommand = new Command<JoinedRunDto>(async (run) => await ViewJoinedRunDetailsAsync(run));
                SearchCommand = new Command<string>((searchText) => FilterRuns(searchText));
            }

            public async Task LoadJoinedRunsAsync()
            {
                if (IsLoading) return;

                try
                {
                    IsLoading = true;
                    Debug.WriteLine("Starting to load runs from API...");

                    // Load runs from API service (with fallback to mock data)
                    var runs = await LoadJoinedRunsFromService();

                    // Clear existing runs and add new ones
                    _runs.Clear();
                    foreach (var run in runs)
                    {
                        _runs.Add(run);
                    }

                    // Apply current filter to display the runs
                    FilterRuns(SearchText);

                    Debug.WriteLine($"Successfully loaded and filtered {runs.Count} runs");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in LoadRunsAsync: {ex.Message}");

                    // Even if there's an error, try to show mock data so UI isn't empty
                    try
                    {
                        var mockRuns = GetMockRuns();
                        _runs.Clear();
                        foreach (var run in mockRuns)
                        {
                            _runs.Add(run);
                        }
                        FilterRuns(SearchText);
                        Debug.WriteLine($"Loaded {mockRuns.Count} mock runs as fallback");
                    }
                    catch (Exception mockEx)
                    {
                        Debug.WriteLine($"Error loading mock data: {mockEx.Message}");
                    }

                    throw; // Re-throw to let the UI handle the error
                }
                finally
                {
                    IsLoading = false;
                }
            }

            public async Task RefreshRunsAsync()
            {
                if (IsRefreshing) return;

                try
                {
                    IsRefreshing = true;
                    await LoadJoinedRunsAsync();
                }
                finally
                {
                    IsRefreshing = false;
                }
            }

            private async Task<List<JoinedRunViewModel>> LoadJoinedRunsFromService()
            {
                var runs = new List<JoinedRunViewModel>();

            
                // Try to get profile service from DI
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                var profileService = serviceProvider.GetService<IJoinedRunService>();

                if (profileService == null)
                {
                    // Fallback if service is not available through DI
                    profileService = new JoinedRunService();
                }

                try
                    {
                        Debug.WriteLine("Loading runs from API service...");

                        // Load from actual API service
                        var serviceRuns = await profileService.GetUserJoinedRunsAsync(App.User?.ProfileId);

                        if (serviceRuns != null && serviceRuns.Any())
                        {
                            runs.AddRange(serviceRuns.Select(MapToViewModel));
                            Debug.WriteLine($"Successfully loaded {runs.Count} runs from API");
                        }
                        else
                        {
                            Debug.WriteLine("API returned empty result, using mock data as fallback");
                            runs = GetMockRuns();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error loading from API service: {ex.Message}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                        // Fall back to mock data only if API fails
                        Debug.WriteLine("Falling back to mock data due to API error");
                        runs = GetMockRuns();
                    }
               

                return runs;
            }

            private JoinedRunViewModel MapToViewModel(JoinedRun domainRun)
            {
                return new JoinedRunViewModel
                {
                    RunId = domainRun.RunId,
                    CourtId = domainRun.Run.Court.CourtId,
                    //ProfileId = domainRun.ProfileId,
                    //Status = domainRun.Status,
                    //RunDate = domainRun.RunDate,
                    //Cost = domainRun.Cost,
                    //Name = domainRun.Name,
                    //Address = domainRun.Court.Address,
                    //City = domainRun.Court.City,
                    //State = domainRun.Court.State,
                    //Zip = domainRun.Court.Zip,
                    //Description = domainRun.Description,
                    //RunTime = domainRun.RunTime,
                    //EndTime = domainRun.EndTime,
                    //Type = domainRun.Type,
                    //CreatedDate = domainRun.CreatedDate,
                    //RunNumber = domainRun.RunNumber,
                    //SkillLevel = domainRun.SkillLevel,
                    //PaymentMethod = domainRun.PaymentMethod,
                    //TeamType = domainRun.TeamType,
                    //PlayerLimit = domainRun.PlayerLimit,
                    Username = "test",

                 
                };
            }

            private double CalculateDistance(string address, string city, string state)
            {
                // TODO: Implement actual distance calculation using user's location
                // For now, return a random distance between 0.5 and 10 miles
                var random = new Random();
                return Math.Round(random.NextDouble() * 9.5 + 0.5, 1);
            }

            private List<JoinedRunViewModel> GetMockRuns()
            {
                return new List<JoinedRunViewModel>
            {
                new JoinedRunViewModel
                {
                    RunId = "1",
                    Name = "Downtown Basketball",
                    Address = "123 Main St",
                    City = "Atlanta",
                    State = "GA",
                    RunDate = DateTime.Today.AddDays(1),
                    RunTime = "6:00 PM",
                    EndTime = "8:00 PM",
                    Cost = 15.00m,
                    PlayerLimit = 10,
                    CurrentPlayerCount = 6,
                    SkillLevel = "Intermediate",
                    TeamType = "5-on-5",
                    Distance = 2.3,
                    HostName = "Mike Johnson",
                    IsPublic = true,
                    Description = "Competitive pickup game"
                },
                new JoinedRunViewModel
                {
                    RunId = "2",
                    Name = "Morning Shootaround",
                    Address = "456 Oak Ave",
                    City = "Atlanta",
                    State = "GA",
                    RunDate = DateTime.Today.AddDays(2),
                    RunTime = "8:00 AM",
                    EndTime = "10:00 AM",
                    Cost = 0m,
                    PlayerLimit = 8,
                    CurrentPlayerCount = 3,
                    SkillLevel = "All Levels",
                    TeamType = "3-on-3",
                    Distance = 1.7,
                    HostName = "Sarah Davis",
                    IsPublic = true,
                    Description = "Casual morning game"
                }
            };
            }

            public void FilterRuns(string searchText)
            {
                if (_runs == null || _runs.Count == 0)
                {
                    Runs = new ObservableCollection<JoinedRunViewModel>();
                    return;
                }

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    Runs = new ObservableCollection<JoinedRunViewModel>(_runs);
                    return;
                }

                searchText = searchText.TrimStart('@').ToLower();
                var filtered = _runs.Where(r =>
                    (r.Name?.ToLower().Contains(searchText) ?? false) ||
                    (r.Address?.ToLower().Contains(searchText) ?? false) ||
                    (r.City?.ToLower().Contains(searchText) ?? false) ||
                    (r.Username?.ToLower().Contains(searchText) ?? false) ||
                    (r.HostName?.ToLower().Contains(searchText) ?? false)
                ).ToList();

                Runs = new ObservableCollection<JoinedRunViewModel>(filtered);
            }

            private async Task JoinedRunAsync(JoinedRunDto run)
            {
                try
                {
                    Debug.WriteLine($"Joining run: {run?.Name}");

                    if (run == null) return;

                    // Check if run is full
                    if (run.CurrentPlayerCount >= run.PlayerLimit)
                    {
                        await Application.Current.MainPage.DisplayAlert("Run Full",
                            "This run is already full. Try another run or contact the host.", "OK");
                        return;
                    }

                    // Join the run (implement actual join logic)
                    bool joinSuccess = await JoinedRunService(run);

                    if (joinSuccess)
                    {
                        run.CurrentPlayerCount++;
                        await Application.Current.MainPage.DisplayAlert("Success",
                            $"You've successfully joined {run.Name}!", "OK");

                        // Refresh the runs to update UI
                        await RefreshRunsAsync();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "Could not join the run. Please try again.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error joining run: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "An error occurred while joining the run.", "OK");
                }
            }

            private async Task<bool> JoinedRunService(JoinedRunDto run)
            {
                try
                {
                    if (_runService != null)
                    {
                       

                        // Call the actual API service to join the run
                        //bool result = await _privateRunService.JoinRunAsync(run.Id, App.User?.UserId);
                        bool result = true;

                        Debug.WriteLine($"API join result: {result}");
                        return result;
                    }
                    else
                    {
                        Debug.WriteLine("PrivateRunService not available, simulating join");
                        // Simulate API call if service not available
                        await Task.Delay(1000);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error joining run via API: {ex.Message}");

                    // Return false if API call fails
                    return false;
                }
            }

            private async Task ViewJoinedRunDetailsAsync(JoinedRunDto run)
            {
                try
                {
                    Debug.WriteLine($"Viewing details for run: {run?.Name}");

                    if (run == null) return;

                    // Navigate to run details page
                   // await Application.Current.MainPage.Navigation.PushAsync(new JoinedRunDetailsPage(run));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error viewing run details: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Could not view run details.", "OK");
                }
            }
        }
    }
    }