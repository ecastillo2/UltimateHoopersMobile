using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper for diagnosing and recovering from page initialization issues
    /// </summary>
    public static class PageInitializationHelper
    {
        // Flag to track if diagnostic mode is active
        private static bool _diagnosticModeActive = false;

        /// <summary>
        /// Enables diagnostic mode for tracking page initialization
        /// </summary>
        public static void EnableDiagnosticMode()
        {
            _diagnosticModeActive = true;
            Debug.WriteLine("PageInitializationHelper: Diagnostic mode enabled");
        }

        /// <summary>
        /// Logs diagnostic information about a page during initialization
        /// </summary>
        public static void LogPageInitialization(Page page, string context)
        {
            if (!_diagnosticModeActive)
                return;

            try
            {
                Debug.WriteLine($"--------- PAGE INITIALIZATION: {context} ---------");
                Debug.WriteLine($"Page Type: {page.GetType().Name}");
                Debug.WriteLine($"Navigation Stack Depth: {page.Navigation?.NavigationStack?.Count ?? 0}");
                Debug.WriteLine($"Is MainPage: {Application.Current?.MainPage == page}");
                Debug.WriteLine($"IsVisible: {page.IsVisible}");
                Debug.WriteLine($"Width: {page.Width}, Height: {page.Height}");
                Debug.WriteLine($"ShellAttached: {Shell.GetNavBarIsVisible(page) != null}");

                // Check if page is inside a Shell
                var parent = page.Parent;
                bool isInShell = false;
                while (parent != null)
                {
                    if (parent is Shell)
                    {
                        isInShell = true;
                        break;
                    }
                    parent = parent.Parent;
                }
                Debug.WriteLine($"Is in Shell: {isInShell}");

                // Report on Shell state if available
                if (Shell.Current != null)
                {
                    Debug.WriteLine($"Shell.Current.Items.Count: {Shell.Current.Items.Count}");
                    Debug.WriteLine($"Shell.Current.CurrentItem?.Route: {Shell.Current.CurrentItem?.Route}");
                    Debug.WriteLine($"Shell.Current.CurrentPage?.GetType().Name: {Shell.Current.CurrentPage?.GetType().Name}");
                }

                Debug.WriteLine("-------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LogPageInitialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to recover a page that may not be initializing properly
        /// </summary>
        public static void AttemptPageRecovery(Page page)
        {
            try
            {
                Debug.WriteLine($"PageInitializationHelper: Attempting recovery for {page.GetType().Name}");

                // Mark the page as visible
                page.IsVisible = true;

                // Request a layout update instead of using ForceLayout which doesn't exist in MAUI
                if (page.Handler != null)
                {
                    page.InvalidateMeasure();
                }

                // If the page has a parent that's a layout container, try to invalidate its measure as well
                if (page.Parent is Layout parentLayout && parentLayout.Handler != null)
                {
                    parentLayout.InvalidateMeasure();
                }

                // If the page is a ContentPage, check its content
                if (page is ContentPage contentPage && contentPage.Content is Layout contentLayout && contentLayout.Handler != null)
                {
                    contentLayout.InvalidateMeasure();
                }

                Debug.WriteLine($"PageInitializationHelper: Recovery attempt completed for {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PageInitializationHelper: Error in recovery attempt: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up event handlers to monitor page lifecycle
        /// </summary>
        public static void MonitorPageLifecycle(Page page)
        {
            if (!_diagnosticModeActive)
                return;

            try
            {
                page.Appearing += (s, e) =>
                {
                    Debug.WriteLine($"PAGE APPEARING: {page.GetType().Name}");
                    LogPageInitialization(page, "Appearing");
                };

                page.Disappearing += (s, e) =>
                {
                    Debug.WriteLine($"PAGE DISAPPEARING: {page.GetType().Name}");
                };

                page.Loaded += (s, e) =>
                {
                    Debug.WriteLine($"PAGE LOADED: {page.GetType().Name}");
                    LogPageInitialization(page, "Loaded");

                    // After load, check if page needs recovery
                    if (page.Width <= 0 || page.Height <= 0 || !page.IsVisible)
                    {
                        Debug.WriteLine($"PAGE MAY NEED RECOVERY: {page.GetType().Name}");
                        AttemptPageRecovery(page);
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PageInitializationHelper: Error setting up lifecycle monitoring: {ex.Message}");
            }
        }
    }
}