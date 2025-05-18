using Domain;
using Domain.DtoModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace UltimateHoopers.ViewModels
{
    public class HooperViewModel : BindableObject
    {
        // Base properties
        public string ProfileId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Position { get; set; }
        public string Location { get; set; }
        public int Rank { get; set; }
        public int GamesPlayed { get; set; }
        public string Record { get; set; }
        public string WinPercentage { get; set; }
        public double Rating { get; set; }
        public string ProfileImage { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string PlayerNumber { get; set; }
        public string Bio { get; set; }
        public string StyleOfPlay { get; set; }

        //public Profile Profile { get; set; }
        public SettingViewModelDto? Setting { get; set; }
        public ScoutingReport? ScoutingReport { get; set; }
        public GameStatistics? GameStatistics { get; set; }
        public string? FollowersCount { get; set; }
        public string? FollowingCount { get; set; }

        // Computed properties
        public string UsernameDisplay => $"@{Username}";
        public string PositionLocation => $"{Position} • {Location}";
        public string RatingDisplay => Rating.ToString("0.0");

        // Profile image handling
        public bool HasValidImage => !string.IsNullOrEmpty(ProfileImage) &&
                                     (ProfileImage.StartsWith("http://") ||
                                      ProfileImage.StartsWith("https://"));

        // Placeholder coloring
        public string Initials { get; private set; }
        public Color InitialsColor { get; private set; }

        public void InitProperties()
        {
            // Generate initials from username
            Initials = !string.IsNullOrEmpty(Username) && Username.Length > 0
                ? Username.Substring(0, Math.Min(2, Username.Length)).ToUpper()
                : "?";

            // Generate consistent color based on username
            InitialsColor = GetUsernameColor(Username);

            // Call property changed for computed properties
            OnPropertyChanged(nameof(UsernameDisplay));
            OnPropertyChanged(nameof(PositionLocation));
            OnPropertyChanged(nameof(RatingDisplay));
            OnPropertyChanged(nameof(HasValidImage));
            OnPropertyChanged(nameof(Initials));
            OnPropertyChanged(nameof(InitialsColor));
        }

        private Color GetUsernameColor(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Colors.DarkViolet;

            // Generate hash from username for consistent color
            int hash = 0;
            foreach (char c in username)
            {
                hash = (hash * 31) + c;
            }

            // Define dark color palette with basketball-inspired colors
            var colors = new List<Color>
            {
                Color.FromArgb("#5E35B1"),  // Dark Violet
                Color.FromArgb("#3949AB"),  // Dark Indigo
                Color.FromArgb("#00897B"),  // Dark Teal
                Color.FromArgb("#43A047"),  // Dark Green
                Color.FromArgb("#E53935"),  // Dark Red
                Color.FromArgb("#FB8C00"),  // Dark Orange
                Color.FromArgb("#8E24AA"),  // Dark Purple
                Color.FromArgb("#1E88E5")   // Dark Blue
            };

            // Pick color based on hash
            return colors[Math.Abs(hash) % colors.Count];
        }
    }
}