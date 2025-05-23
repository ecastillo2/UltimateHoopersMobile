using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UltimateHoopers.Models
{
    public class RunDto
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
        public RunDto()
        {
            // Initialize with sample players if not already set
            if (Players == null || Players.Count == 0)
            {
                Players = new ObservableCollection<Player>();

                // Add host as first player
                if (!string.IsNullOrEmpty(HostName))
                {
                    Players.Add(new Player
                    {
                        Id = HostId,
                        Name = HostName,
                        Username = "@" + HostName.ToLower().Replace(" ", ""),
                        ProfileImageUrl = "https://via.placeholder.com/200x200.png?text=" + Uri.EscapeDataString(HostName.Substring(0, 1)),
                        IsHost = true,
                        HasJoined = true
                    });
                }

                // Add sample players based on CurrentPlayerCount
                string[] sampleNames = { "Michael", "Sarah", "Jason", "Tiffany", "Carlos", "Zoe", "David", "Maya", "Kevin", "Lisa" };
                for (int i = Players.Count; i < Math.Min(CurrentPlayerCount, sampleNames.Length + 1); i++)
                {
                    string playerName = sampleNames[i - (Players.Count > 0 ? 1 : 0)];
                    Players.Add(new Player
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = playerName,
                        Username = "@" + playerName.ToLower(),
                        ProfileImageUrl = "https://via.placeholder.com/200x200.png?text=" + Uri.EscapeDataString(playerName.Substring(0, 1)),
                        IsHost = false,
                        HasJoined = true
                    });
                }

                // Add more generic players if needed
                for (int i = Players.Count; i < CurrentPlayerCount; i++)
                {
                    Players.Add(new Player
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"Player {i + 1}",
                        Username = $"@player{i + 1}",
                        IsHost = false,
                        HasJoined = true
                    });
                }
            }
        }

        // Get players who have actually joined the run
        public ObservableCollection<Player> GetJoinedPlayers()
        {
            return new ObservableCollection<Player>(
                Players.Where(p => p.HasJoined).ToList()
            );
        }
    }

    // Standard player class without BindableObject dependency
    public class Player
    {
        // Basic properties
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsHost { get; set; }
        public bool HasJoined { get; set; }

        // Property to check if there's a valid profile image URL
        public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);
    }
}