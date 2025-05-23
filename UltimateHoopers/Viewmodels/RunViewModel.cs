using Domain;
using Domain.DtoModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimateHoopers.Models;

namespace UltimateHoopers.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a Run on the UI
    /// </summary>
    public class RunViewModel : BindableObject
    {
        // Basic properties
        public string RunId { get; set; }
        public string Name { get; set; }
        public DateTime? RunDate { get; set; }
        public string RunTime { get; set; }
        public string EndTime { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public decimal? Cost { get; set; }
        public int? PlayerLimit { get; set; }
        public int CurrentPlayerCount { get; set; }
        public string CourtImageUrl { get; set; }
        public string HostName { get; set; }
        public string HostId { get; set; }
        public string SkillLevel { get; set; }
        public string GameType { get; set; }
        public bool IsPublic { get; set; }
        public double Distance { get; set; }

        // Computed properties for UI display
        public string FormattedDate => RunDate?.ToString("dddd, MMMM dd, yyyy") ?? "TBD";
        public string DayOfMonth => RunDate?.Day.ToString() ?? "?";
        public string Month => RunDate?.ToString("MMM")?.ToUpper() ?? "TBD";
        public string PlayerCountDisplay => $"{CurrentPlayerCount}/{PlayerLimit ?? 0} players";
        public string DistanceText => $"{Distance:F1} miles away";
        public bool IsFull => CurrentPlayerCount >= (PlayerLimit ?? 0);
        public string CostText => Cost > 0 ? $"${Cost:F2}" : "Free";
        public string Time => $"{RunTime} - {EndTime}";
        public string Location => $"{Address}, {City}, {State}";

        /// <summary>
        /// Creates a RunViewModel from a Run domain model
        /// </summary>
        public static RunViewModel FromRun(Run run, Court court = null)
        {
            if (run == null) return null;

            return new RunViewModel
            {
                RunId = run.RunId,
                Name = run.Name ?? "Basketball Run",
                RunDate = run.RunDate,
                RunTime = run.RunTime ?? "TBD",
                EndTime = run.EndTime ?? "TBD",
                Address = court?.Address ?? "Address not available",
                City = court?.City ?? "City not available",
                State = court?.State ?? "State not available",
                Description = run.Description ?? "No description available",
                Cost = run.Cost ?? 0.00m,
                PlayerLimit = run.PlayerLimit ?? 10,
                CurrentPlayerCount = run.PlayerCount ?? 0,
                CourtImageUrl = court?.ImageURL ?? "https://placehold.co/600x400/png?text=Basketball+Court",
                HostName = "Host", // Would need to be retrieved from the profile service
                HostId = run.ProfileId ?? "",
                SkillLevel = run.SkillLevel ?? "All Levels",
                GameType = run.TeamType ?? "5-on-5",
                IsPublic = run.IsPublic ?? true
            };
        }

        /// <summary>
        /// Creates a RunViewModel from a JoinedRunDetailViewModelDto
        /// </summary>
        public static RunViewModel FromJoinedRunDto(JoinedRunDetailViewModelDto joinedRun)
        {
            if (joinedRun?.Run == null) return null;

            var run = joinedRun.Run;
            var court = run.Court;

            return new RunViewModel
            {
                RunId = run.RunId,
                Name = run.Name ?? "Basketball Run",
                RunDate = run.RunDate,
                RunTime = run.RunTime ?? "TBD",
                EndTime = run.EndTime ?? "TBD",
                Address = court?.Address ?? "Address not available",
                City = court?.City ?? "City not available",
                State = court?.State ?? "State not available",
                Description = run.Description ?? "No description available",
                Cost = run.Cost ?? 0.00m,
                PlayerLimit = run.PlayerLimit ?? 10,
                CurrentPlayerCount = run.PlayerCount ?? 0,
                CourtImageUrl = court?.ImageURL ?? "https://placehold.co/600x400/png?text=Basketball+Court",
                HostName = "Host", // Would need to be retrieved from the profile service
                HostId = run.ProfileId ?? "",
                SkillLevel = run.SkillLevel ?? "All Levels",
                GameType = run.TeamType ?? "5-on-5",
                IsPublic = run.IsPublic ?? true
            };
        }

        /// <summary>
        /// Converts this RunViewModel to a RunDto for navigation
        /// </summary>
        public RunDto ToRunModel()
        {
            var run = new RunDto
            {
                Id = RunId ?? Guid.NewGuid().ToString(),
                Name = Name ?? "Basketball Run",
                Location = Name ?? "Court",
                Address = Address ?? "Location TBD",
                Date = RunDate ?? DateTime.Now.AddDays(1),
                Time = Time,
                HostName = HostName ?? "Host",
                HostId = HostId ?? "",
                SkillLevel = SkillLevel ?? "All Levels",
                GameType = GameType ?? "5-on-5",
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
    /// Helper class for cursor-based pagination with runs
    /// </summary>
    internal class CourtCursorData
    {
        public string Id { get; set; }
        public string Zip { get; set; }
        public string Status { get; set; }
    }
}