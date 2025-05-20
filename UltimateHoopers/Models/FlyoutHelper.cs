using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper class to ensure proper Shell Flyout (hamburger menu) behavior
    /// </summary>
    public static class FlyoutHelper
    {
        /// <summary>
        /// Ensures that the hamburger menu (Flyout) is enabled for the current page
        /// </summary>
        /// <param name="page">The page to configure</param>
        public static void EnableFlyoutMenu(Page page)
        {
            try
            {
                if (page == null)
                    return;

                // Set the FlyoutBehavior to Flyout (hamburger menu)
                Shell.SetFlyoutBehavior(page, FlyoutBehavior.Flyout);
                Debug.WriteLine($"FlyoutHelper: Enabled hamburger menu for {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FlyoutHelper: Error enabling hamburger menu: {ex.Message}");
            }
        }

        /// <summary>
        /// Disables the hamburger menu for the current page
        /// </summary>
        /// <param name="page">The page to configure</param>
        public static void DisableFlyoutMenu(Page page)
        {
            try
            {
                if (page == null)
                    return;

                // Set the FlyoutBehavior to Disabled (no hamburger menu)
                Shell.SetFlyoutBehavior(page, FlyoutBehavior.Disabled);
                Debug.WriteLine($"FlyoutHelper: Disabled hamburger menu for {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FlyoutHelper: Error disabling hamburger menu: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures all pages in the application to show the hamburger menu
        /// This should be called after logging in
        /// </summary>
        public static void EnableFlyoutMenuGlobally()
        {
            try
            {
                if (Shell.Current == null)
                {
                    Debug.WriteLine("FlyoutHelper: Shell.Current is null, cannot enable flyout globally");
                    return;
                }

                // Set the Shell's FlyoutBehavior property
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                Debug.WriteLine("FlyoutHelper: Enabled hamburger menu globally");

                // Also set the FlyoutIsPresented to false to ensure it starts closed
                Shell.Current.FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FlyoutHelper: Error enabling global hamburger menu: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the Flyout header text with the user's information
        /// </summary>
        /// <param name="username">The username to display</param>
        public static void UpdateFlyoutHeader(string username)
        {
            try
            {
                if (Shell.Current == null)
                {
                    Debug.WriteLine("FlyoutHelper: Shell.Current is null, cannot update flyout header");
                    return;
                }

                // Find the Label in the FlyoutHeader that displays the welcome message
                var flyoutHeader = Shell.Current.FlyoutHeader;
                if (flyoutHeader == null)
                {
                    Debug.WriteLine("FlyoutHelper: FlyoutHeader is null");
                    return;
                }

                // Try to find the welcome Label in the FlyoutHeader
                if (flyoutHeader is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is VerticalStackLayout stack)
                        {
                            foreach (var stackChild in stack.Children)
                            {
                                if (stackChild is Label label && label.Text != null && label.Text.StartsWith("Welcome"))
                                {
                                    // Update the label text
                                    label.Text = $"Welcome, {username}!";
                                    Debug.WriteLine($"FlyoutHelper: Updated welcome message to '{label.Text}'");
                                    return;
                                }
                            }
                        }
                    }
                }

                Debug.WriteLine("FlyoutHelper: Could not find welcome Label in FlyoutHeader");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FlyoutHelper: Error updating flyout header: {ex.Message}");
            }
        }
    }
}