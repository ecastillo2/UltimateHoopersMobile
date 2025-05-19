using Microsoft.Maui.Controls;
using System;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper class for common UI configuration operations
    /// </summary>
    public static class UIConfigHelper
    {
        /// <summary>
        /// Configures common page settings when a page appears
        /// </summary>
        /// <param name="page">The page to configure</param>
        public static void ConfigurePageOnAppearing(Page page)
        {
            if (page == null)
                return;

            try
            {
                // Ensure navigation bar is hidden if needed
                if (Shell.GetNavBarIsVisible(page) == false)
                {
                    Shell.SetNavBarIsVisible(page, false);
                }

                // Additional configuration can be added here as needed
            }
            catch (Exception ex)
            {
                // Log the error but don't crash
                Console.WriteLine($"Error in ConfigurePageOnAppearing: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the status bar color based on the page background
        /// </summary>
        /// <param name="page">The page whose status bar to update</param>
        /// <param name="isDarkBackground">Whether the background is dark (for light status bar text)</param>
        public static void UpdateStatusBarColor(Page page, bool isDarkBackground)
        {
            // This would be implemented with platform-specific code
            // For example, on Android you would use:
            // Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#yourColor"), !isDarkBackground);

            // Since this requires platform-specific implementations, we'll just leave it as a placeholder
            // In a real implementation, you would use dependency service or platform-specific code
        }
    }
}