using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper class to manage global page settings across the app
    /// </summary>
    public static class GlobalPageHelper
    {
        /// <summary>
        /// Applies common settings to all pages, such as hiding the navigation bar
        /// </summary>
        /// <param name="page">The page to configure</param>
        public static void ConfigurePage(Page page)
        {
            try
            {
                // Hide the navigation bar for all pages
                Shell.SetNavBarIsVisible(page, false);

                // Clear the title
                page.Title = string.Empty;

                Debug.WriteLine($"GlobalPageHelper: Applied global settings to {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GlobalPageHelper: Error applying global settings to {page.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers global handlers to automatically apply settings to all pages
        /// </summary>
        public static void RegisterGlobalHandlers()
        {
            try
            {
                // Hook into the Navigation event to catch all page navigations
                Shell.Current.Navigating += Current_Navigating;

                Debug.WriteLine("GlobalPageHelper: Registered global navigation handlers");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GlobalPageHelper: Error registering global handlers: {ex.Message}");
            }
        }

        private static void Current_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            try
            {
                // Get the current page after navigation
                var currentPage = Shell.Current?.CurrentPage;

                if (currentPage != null)
                {
                    // Apply settings to the new page
                    ConfigurePage(currentPage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GlobalPageHelper: Error in navigation handler: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Base class that all pages can inherit from to automatically hide the navigation bar
    /// </summary>
    public class NoTitleBarContentPage : ContentPage
    {
        public NoTitleBarContentPage()
        {
            // Hide the navigation bar in the constructor
            Shell.SetNavBarIsVisible(this, false);
            this.Title = string.Empty;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Ensure navigation bar stays hidden even if other code tries to show it
            Shell.SetNavBarIsVisible(this, false);
        }
    }
}