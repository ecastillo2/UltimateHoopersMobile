using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Common.Utilities
{
    /// <summary>
    /// Utility class for formatting dates and calculating relative time expressions
    /// </summary>
    public static class DateTimeUtilities
    {


        public static string FormatTimeSpanTo12Hour(TimeSpan timeSpan)
        {
            try
            {
                // Convert TimeSpan to DateTime for easier formatting
                DateTime dateTime = DateTime.Today.Add(timeSpan);

                // Format to 12-hour time with AM/PM
                return dateTime.ToString("h:mm tt");
            }
            catch (Exception ex)
            {
                //LogError("Error formatting TimeSpan to 12-hour format", ex);
                return timeSpan.ToString(@"hh\:mm");
            }
        }

        /// <summary>
        /// Gets a human-readable string for the relative time between two dates
        /// </summary>
        /// <param name="dateTime">The date to compare</param>
        /// <param name="userTimeZoneId">The timezone ID of the user</param>
        /// <returns>A human-readable relative time string (e.g., "Just now", "5 minutes ago")</returns>
        public static string GetRelativeTime(DateTime dateTime, string userTimeZoneId)
        {
            // Get the user's time zone
            TimeZoneInfo userTimeZone;
            try
            {
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Default to UTC if the timezone is not found
                userTimeZone = TimeZoneInfo.Utc;
            }
            catch (InvalidTimeZoneException)
            {
                // Default to UTC if the timezone is invalid
                userTimeZone = TimeZoneInfo.Utc;
            }

            // Ensure the input date is in UTC if it's not already
            DateTime dateTimeUtc = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : TimeZoneInfo.ConvertTimeToUtc(dateTime, userTimeZone);

            // Convert the UTC date to the user's local time
            DateTime dateTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, userTimeZone);

            // Get the current UTC time and convert it to the user's local time
            DateTime currentUserTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);

            // Calculate the time difference from the user's current time
            TimeSpan timeDifference = currentUserTime.Subtract(dateTimeInUserTimeZone);

            // Check if the event was in the future (just in case)
            if (timeDifference.TotalSeconds < 0)
                return "Just now";

            // Handle different time frames
            if (timeDifference.TotalMinutes < 1)
            {
                return "Just now";
            }
            else if (timeDifference.TotalMinutes < 60)
            {
                int minutes = (int)timeDifference.TotalMinutes;
                return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
            }
            else if (timeDifference.TotalHours < 24)
            {
                int hours = (int)timeDifference.TotalHours;
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }
            else if (timeDifference.TotalDays < 7)
            {
                int days = (int)timeDifference.TotalDays;
                return $"{days} {(days == 1 ? "day" : "days")} ago";
            }
            else
            {
                // For dates older than a week, return the actual date in user's local format
                return dateTimeInUserTimeZone.ToString("MMM dd, yy", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Formats a date to a user-friendly string
        /// </summary>
        /// <param name="dateTime">The date to format</param>
        /// <param name="format">The format to use (optional)</param>
        /// <returns>A formatted date string</returns>
        public static string FormatDate(DateTime dateTime, string format = "MMM dd, yyyy")
        {
            return dateTime.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to parse a string to a DateTime with various formats
        /// </summary>
        /// <param name="dateString">The date string to parse</param>
        /// <param name="result">The parsed DateTime if successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParseDate(string dateString, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                result = DateTime.MinValue;
                return false;
            }

            // Try common date formats
            string[] formats = {
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "MM/dd/yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "MMMM dd, yyyy",
                "dd MMMM yyyy"
            };

            return DateTime.TryParseExact(
                dateString,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result);
        }
    }

    /// <summary>
    /// Utility class for formatting ranking values
    /// </summary>
    public static class RankingUtilities
    {
        /// <summary>
        /// Gets the ordinal suffix for a ranking number
        /// </summary>
        /// <param name="ranking">The ranking number</param>
        /// <returns>The ranking with ordinal suffix (e.g., "1st", "2nd", "3rd")</returns>
        public static string GetOrdinalSuffix(int ranking)
        {
            if (ranking <= 0 || ranking > 100)
                throw new ArgumentOutOfRangeException(nameof(ranking), "Ranking must be between 1 and 100.");

            if (ranking % 100 >= 11 && ranking % 100 <= 13)
                return $"{ranking}th";

            return ranking switch
            {
                _ when ranking % 10 == 1 => $"{ranking}st",
                _ when ranking % 10 == 2 => $"{ranking}nd",
                _ when ranking % 10 == 3 => $"{ranking}rd",
                _ => $"{ranking}th",
            };
        }

        /// <summary>
        /// Converts a ranking string (e.g., "1st", "2nd", "3rd") to integer value
        /// </summary>
        /// <param name="ranking">The ranking string</param>
        /// <returns>The integer value of the ranking</returns>
        public static int GetRankingValue(string ranking)
        {
            if (string.IsNullOrEmpty(ranking))
                return int.MaxValue; // Handle any empty/invalid rankings gracefully

            // Remove "st", "nd", "rd", "th" from the ranking string
            var numericRank = new string(Regex.Match(ranking, @"\d+").Value.ToCharArray());
            return int.TryParse(numericRank, out var result) ? result : int.MaxValue;
        }
    }
}