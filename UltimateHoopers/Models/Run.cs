using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UltimateHoopers.Models
{
    public class Run
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string HostName { get; set; }
        public string HostId { get; set; }
        public string SkillLevel { get; set; }
        public string GameType { get; set; }
        public bool IsPublic { get; set; }
        public string Description { get; set; }
        public int PlayerLimit { get; set; }
        public int CurrentPlayerCount { get; set; }
        public string CourtImageUrl { get; set; }
        public decimal Cost { get; set; }
        public double Distance { get; set; }
        public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();

        // Computed properties
        public string FormattedDate => Date.ToString("dddd, MMMM dd, yyyy");
        public string DayOfMonth => Date.Day.ToString();
        public string Month => Date.ToString("MMM").ToUpper();
        public string PlayerCountDisplay => $"{CurrentPlayerCount}/{PlayerLimit} players";
        public string DistanceText => $"{Distance:F1} miles away";
        public bool IsFull => CurrentPlayerCount >= PlayerLimit;
        public string CostText => Cost > 0 ? $"${Cost:F2}" : "Free";
        public string PrivacyText => IsPublic ? "Public" : "Private";
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsHost { get; set; }
    }
}