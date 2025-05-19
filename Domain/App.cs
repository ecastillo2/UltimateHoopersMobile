using System;

namespace Domain
{
    /// <summary>
    /// Static class providing application-wide properties and services
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Gets or sets the current user ID
        /// </summary>
        public static string CurrentUserId { get; set; }

        /// <summary>
        /// Initializes the App with the given user ID
        /// </summary>
        /// <param name="userId">The user ID to set as current</param>
        public static void Initialize(string userId)
        {
            CurrentUserId = userId;
        }
    }
}