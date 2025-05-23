using Domain.DtoModel;
using System;
using System.Linq;
using UltimateHoopers.Models;

namespace UltimateHoopers.ViewModels
{
    public class RunViewModel
    {
        // Base properties
        public string RunId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DayOfMonth { get; set; }
        public string Month { get; set; }
        public string Time { get; set; }
        public int CurrentPlayerCount { get; set; }
        public int PlayerLimit { get; set; }

        // Display properties
        public string PlayerCountDisplay => $"{CurrentPlayerCount}/{PlayerLimit}";

        // Static converter method to create RunViewModel from JoinedRunDto
        public static RunViewModel FromJoinedRunDto(JoinedRunDetailViewModelDto dto)
        {
            if (dto == null || dto.Run == null)
                return null;

            try
            {
                // Default to current date
                DateTime runDate = DateTime.Now;

                // Safely extract the date using ToString()
                try
                {
                    // Get a string representation and try to parse it
                    // This avoids type-checking problems completely
                    var dateStr = dto.Run.RunDate?.ToString();
                    if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
                    {
                        runDate = parsedDate;
                    }
                }
                catch (Exception ex)
                {
                    // Just log and continue with default date
                    System.Diagnostics.Debug.WriteLine($"Date parsing error: {ex.Message}");
                }

                return new RunViewModel
                {
                    RunId = dto.Run.RunId,
                    Name = dto.Run.Name ?? "Basketball Run",
                    Address = dto.Run.Court?.Address ?? "Address not available",
                    DayOfMonth = runDate.Day.ToString(),
                    Month = runDate.ToString("MMM").ToUpper(),
                    Time = $"{dto.Run.RunTime ?? "TBD"} - {dto.Run.EndTime ?? "TBD"}",
                    CurrentPlayerCount = dto.Run.JoinedRunList?.Count ?? 0,
                    PlayerLimit = dto.Run.PlayerLimit ?? 10
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating RunViewModel from JoinedRunDto: {ex.Message}");
                return null;
            }
        }

        // Helper method to convert to RunDto
        public RunDto ToRunModel()
        {
            return new RunDto
            {
                Id = RunId,
                Name = Name,
                Address = Address,
                PlayerLimit = PlayerLimit,
                CurrentPlayerCount = CurrentPlayerCount,
                Date = ParseRunDate(),
                Time = Time
            };
        }

        // Helper to parse run date from DayOfMonth and Month
        private DateTime ParseRunDate()
        {
            try
            {
                int day = int.Parse(DayOfMonth);
                string monthStr = Month.Substring(0, 3); // First 3 chars

                // Find the month number
                string[] months = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
                int month = Array.IndexOf(months, monthStr) + 1;

                if (month < 1) month = DateTime.Now.Month; // Fallback to current month

                // Use current year, or next year if the date is in the past
                int year = DateTime.Now.Year;
                if (month < DateTime.Now.Month || (month == DateTime.Now.Month && day < DateTime.Now.Day))
                {
                    year++; // Next year
                }

                return new DateTime(year, month, day);
            }
            catch
            {
                // Return tomorrow if parsing fails
                return DateTime.Now.AddDays(1);
            }
        }
    }
}