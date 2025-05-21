using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace UltimateHoopers.Models
{
    public class Run
    {
        // Basic Info
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }

        // Date and Time
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string DayOfMonth => Date.Day.ToString();
        public string Month => Date.ToString("MMM");
        public string FormattedDate => Date.ToString("dddd, MMMM d, yyyy");

        // Game Details
        public string HostName { get; set; }
        public string HostId { get; set; }
        public string SkillLevel { get; set; } = "All Levels";
        public string GameType { get; set; } = "5-on-5";
        public bool IsPublic { get; set; } = true;
        public string Description { get; set; }

        // Player management
        public int PlayerLimit { get; set; } = 10;
        public int CurrentPlayerCount { get; set; }
        public bool IsFull => CurrentPlayerCount >= PlayerLimit;
        public string PlayerCountDisplay => $"{CurrentPlayerCount}/{PlayerLimit} players";

        // Court Information
        public string CourtImageUrl { get; set; }
        public double CourtLatitude { get; set; }
        public double CourtLongitude { get; set; }
        public decimal Cost { get; set; }

        // UI Helpers
        public bool HasImage => !string.IsNullOrEmpty(CourtImageUrl);
        public double Distance { get; set; } // Distance from user's location in miles
        public string DistanceText => $"{Distance:F1} mi";

        // Status
        public string Status { get; set; } = "Active";
        public bool IsActive => Status == "Active";

        // Players
        public List<Player> Players { get; set; } = new List<Player>();
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}