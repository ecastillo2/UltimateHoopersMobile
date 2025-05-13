
namespace Common
{
    public static class RelativeTime
    {
        /// <summary>
        /// Get Relative Time
        /// </summary>
        /// <param name="postDate"></param>
        /// <param name="userTimeZoneId"></param>
        /// <returns></returns>
        public static string GetRelativeTime(DateTime postDate, string userTimeZoneId)
        {
            // Get the user's time zone
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            // Convert postDate to UTC if it's not already in UTC
            DateTime postDateUtc = TimeZoneInfo.ConvertTimeToUtc(postDate, userTimeZone);

            // Convert the UTC postDate to the user's local time
            DateTime postDateInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(postDateUtc, userTimeZone);

            // Get the current UTC time and convert it to the user's local time
            DateTime currentUserTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);

            // Calculate the time difference from the user's current time
            TimeSpan timeDifference = currentUserTime.Subtract(postDateInUserTimeZone);

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
                return $"{(int)timeDifference.TotalMinutes} minutes ago";
            }
            else if (timeDifference.TotalHours < 24)
            {
                return $"{(int)timeDifference.TotalHours} hours ago";
            }
            else if (timeDifference.TotalDays < 7)
            {
                return $"{(int)timeDifference.TotalDays} days ago";
            }
            else
            {
                // For dates older than a week, return the actual date in user's local format
                return postDateInUserTimeZone.ToString("MMM dd, yy");
            }
        }
    }
}
