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
    /// <summary>
    /// ViewModel for individual Private Run items - represents a single run
    /// </summary>
    public class PrivateRunViewModel : BindableObject
    {
        // Base properties from Domain.PrivateRun
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
        public Run ToRunModel()
        {
            var run = new Run
            {
                Id = PrivateRunId ?? Guid.NewGuid().ToString(),
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

            // Initialize Players collection
            run.Players = new ObservableCollection<Player>();

            return run;
        }
    }

    /// <summary>
    /// ViewModel for the Private Run collection/list page
    /// Note: The actual implementation is now in FindRunsPage.xaml.cs as FindRunsViewModel
    /// This class is kept for backward compatibility
    /// </summary>
    [Obsolete("Use FindRunsViewModel in FindRunsPage.xaml.cs instead")]
    public class PrivateRunsViewModel : BindableObject
    {
        private ObservableCollection<PrivateRunViewModel> _runs = new ObservableCollection<PrivateRunViewModel>();
        private bool _isLoading;

        public ObservableCollection<PrivateRunViewModel> Runs
        {
            get => _runs;
            private set
            {
                _runs = value;
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

        public bool IsNotLoading => !_isLoading;

        

        public void FilterRuns(string searchText)
        {
            if (_runs == null || _runs.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                Runs = new ObservableCollection<PrivateRunViewModel>(_runs);
                return;
            }

            searchText = searchText.TrimStart('@').ToLower();
            var filtered = _runs.Where(r =>
                (r.Name?.ToLower().Contains(searchText) ?? false) ||
                (r.Address?.ToLower().Contains(searchText) ?? false) ||
                (r.Username?.ToLower().Contains(searchText) ?? false)
            ).ToList();

            Runs = new ObservableCollection<PrivateRunViewModel>(filtered);
        }
    }
}