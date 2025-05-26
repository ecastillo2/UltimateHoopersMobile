using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Domain;
using System.Diagnostics;

namespace UltimateHoopers.ViewModels
{
    public class CreateRunViewModel : BindableObject
    {
        #region Private Fields
        private bool _hasUnsavedChanges = false;
        private string _courtSearchText;
        private Court _selectedCourt;
        private DateTime _runDate = DateTime.Now.AddDays(1);
        private TimeSpan _startTime = new TimeSpan(18, 0, 0); // 6:00 PM
        private TimeSpan _endTime = new TimeSpan(20, 0, 0); // 8:00 PM
        private string _selectedSkillLevel = "All Levels";
        private string _selectedGameType = "5-on-5";
        private int _playerLimit = 10;
        private bool _hasCost = false;
        private decimal _cost = 0;
        private string _description = string.Empty;
        private bool _isPublic = true;
        private bool _useDirectInvites = true;
        private bool _useInviteCode = false;
        private string _password = string.Empty;
        private bool _isBusy = false;
        private bool _canCreate = false;
        #endregion

        #region Public Properties
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                OnPropertyChanged();
            }
        }

        public string CourtSearchText
        {
            get => _courtSearchText;
            set
            {
                _courtSearchText = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public Court SelectedCourt
        {
            get => _selectedCourt;
            set
            {
                _selectedCourt = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
                ValidateCanCreate();
            }
        }

        public DateTime RunDate
        {
            get => _runDate;
            set
            {
                _runDate = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
                ValidateCanCreate();
            }
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
                ValidateCanCreate();
            }
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public string SelectedSkillLevel
        {
            get => _selectedSkillLevel;
            set
            {
                _selectedSkillLevel = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public string SelectedGameType
        {
            get => _selectedGameType;
            set
            {
                _selectedGameType = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public int PlayerLimit
        {
            get => _playerLimit;
            set
            {
                _playerLimit = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public bool HasCost
        {
            get => _hasCost;
            set
            {
                _hasCost = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
                // If cost is turned off, reset cost to 0
                if (!value)
                {
                    Cost = 0;
                }
            }
        }

        public decimal Cost
        {
            get => _cost;
            set
            {
                _cost = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public bool IsPublic
        {
            get => _isPublic;
            set
            {
                _isPublic = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public bool UseDirectInvites
        {
            get => _useDirectInvites;
            set
            {
                _useDirectInvites = value;
                OnPropertyChanged();
                if (value)
                {
                    UseInviteCode = false;
                }
                HasUnsavedChanges = true;
            }
        }

        public bool UseInviteCode
        {
            get => _useInviteCode;
            set
            {
                _useInviteCode = value;
                OnPropertyChanged();
                if (value)
                {
                    UseDirectInvites = false;
                }
                HasUnsavedChanges = true;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                // Disable create button when busy
                ValidateCanCreate();
            }
        }

        public bool CanCreate
        {
            get => _canCreate;
            set
            {
                _canCreate = value;
                OnPropertyChanged();
            }
        }

        public DateTime MinimumDate => DateTime.Today;

        public ObservableCollection<Court> Courts { get; } = new ObservableCollection<Court>();

        public List<string> SkillLevels { get; } = new List<string>
        {
            "All Levels",
            "Beginner",
            "Intermediate",
            "Advanced"
        };

        public List<string> GameTypes { get; } = new List<string>
        {
            "5-on-5",
            "4-on-4",
            "3-on-3",
            "2-on-2",
            "1-on-1"
        };
        #endregion

        #region Commands
        public ICommand SearchCourtsCommand { get; }
        public ICommand CreateRunCommand { get; }
        #endregion

        public CreateRunViewModel()
        {
            // Initialize commands
            SearchCourtsCommand = new Command(SearchCourts);
            CreateRunCommand = new Command(async () => await CreateRun());

            // Set initial state
            LoadSampleCourts();
            ValidateCanCreate();
        }

        #region Private Methods
        private void ValidateCanCreate()
        {
            // Validate minimum required fields to create a run
            CanCreate = !IsBusy &&
                        SelectedCourt != null &&
                        RunDate >= DateTime.Today &&
                        PlayerLimit >= 2;
        }

        private void SearchCourts()
        {
            try
            {
                // In a real app, this would search for courts based on the search text
                // For demo purposes, just load sample courts
                Courts.Clear();
                LoadSampleCourts();

                // If search text is provided, filter the courts
                if (!string.IsNullOrWhiteSpace(CourtSearchText))
                {
                    // For demo, just show a message
                    Debug.WriteLine($"Searching for courts with: {CourtSearchText}");

                    // You would typically filter the courts based on the search text
                    // For demo, just select the first court if available
                    if (Courts.Count > 0)
                    {
                        SelectedCourt = Courts[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching courts: {ex.Message}");
            }
        }

        private void LoadSampleCourts()
        {
            // Sample court data - in a real app, this would come from a service
            var courts = new List<Court>
            {
                new Court { CourtId = "1", Name = "Downtown Court", Address = "123 Main St", City = "Atlanta", State = "GA", Zip = "30303" },
                new Court { CourtId = "2", Name = "Westside Rec Center", Address = "456 West Ave", City = "Atlanta", State = "GA", Zip = "30318" },
                new Court { CourtId = "3", Name = "Eastside Community Center", Address = "789 East Blvd", City = "Atlanta", State = "GA", Zip = "30307" },
                new Court { CourtId = "4", Name = "University Gym", Address = "101 College Dr", City = "Atlanta", State = "GA", Zip = "30332" },
                new Court { CourtId = "5", Name = "Central Park Courts", Address = "202 Park Way", City = "Atlanta", State = "GA", Zip = "30309" }
            };

            foreach (var court in courts)
            {
                Courts.Add(court);
            }
        }

        private async Task CreateRun()
        {
            if (!CanCreate || IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Validate inputs
                if (SelectedCourt == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Please select a court for the run",
                        "OK");
                    return;
                }

                if (RunDate < DateTime.Today)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Run date must be in the future",
                        "OK");
                    return;
                }

                // Create the private run object
                var privateRun = new Run
                {
                    RunId = Guid.NewGuid().ToString(),
                    CourtId = SelectedCourt.CourtId,
                    ProfileId = App.User?.ProfileId,
                    RunDate = RunDate.Add(StartTime),
                    Status = "Active",
                    Cost = HasCost ? Cost : 0,
                    Description = Description,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Type = !IsPublic ? "Private" : "Public",
                    CreatedDate = DateTime.Now,
                    RunNumber = new Random().Next(1000, 9999).ToString(),
                    SkillLevel = SelectedSkillLevel,
                    TeamType = SelectedGameType,
                    PlayerLimit = PlayerLimit
                };

                // In a real app, this would call a service to create the run
                // await _privateRunService.CreatePrivateRunAsync(privateRun);

                // For demo purposes, just show a success message
                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    $"Run created at {SelectedCourt.Name} on {RunDate.ToShortDateString()} at {StartTime:hh\\:mm}!",
                    "OK");

                // Reset has unsaved changes flag
                HasUnsavedChanges = false;

                // Navigate back or to run details
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating run: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "There was a problem creating your run. Please try again.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion
    }
}