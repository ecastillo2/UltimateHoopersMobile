
namespace Common
{
    public static class RelativeTime
    {
        /// <summary>
        /// Get Relative Time without  conversion
        /// </summary>
        /// <param name="postDate"></param>
        /// <returns></returns>
        public static string GetRelativeTime(DateTime postDate)
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Calculate the time difference
            TimeSpan timeDifference = currentTime.Subtract(postDate);

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
                // For dates older than a week, return the actual date
                return postDate.ToString("MMM dd, yy");
            }
        }

    }
}
