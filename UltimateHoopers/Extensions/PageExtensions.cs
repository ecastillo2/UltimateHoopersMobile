using Microsoft.Maui.Controls;
using System;

namespace UltimateHoopers.Extensions
{
    /// <summary>
    /// Extension methods for working with Pages in MAUI
    /// </summary>
    public static class PageExtensions
    {
        /// <summary>
        /// Executes an action when a page appears
        /// </summary>
        /// <param name="page">The page to attach the action to</param>
        /// <param name="action">The action to execute when the page appears</param>
        public static void OnAppearing(this Page page, Action action)
        {
            // Store the original appearing handler if it exists
            EventHandler originalHandler = null;

            // Create a new handler that calls both the original and the new action
            void appearingHandler(object sender, EventArgs e)
            {
                // Call the original handler if it exists
                originalHandler?.Invoke(sender, e);

                // Call the new action
                action?.Invoke();
            }

            // Check if there's already a handler
            var appearingField = typeof(Page).GetField("Appearing", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (appearingField != null)
            {
                // Store the original handler
                originalHandler = appearingField.GetValue(page) as EventHandler;
            }

            // Remove any existing handler and add the new one
            page.Appearing -= appearingHandler;
            page.Appearing += appearingHandler;
        }

        /// <summary>
        /// Executes an action when a page disappears
        /// </summary>
        /// <param name="page">The page to attach the action to</param>
        /// <param name="action">The action to execute when the page disappears</param>
        public static void OnDisappearing(this Page page, Action action)
        {
            // Store the original disappearing handler if it exists
            EventHandler originalHandler = null;

            // Create a new handler that calls both the original and the new action
            void disappearingHandler(object sender, EventArgs e)
            {
                // Call the original handler if it exists
                originalHandler?.Invoke(sender, e);

                // Call the new action
                action?.Invoke();
            }

            // Check if there's already a handler
            var disappearingField = typeof(Page).GetField("Disappearing", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (disappearingField != null)
            {
                // Store the original handler
                originalHandler = disappearingField.GetValue(page) as EventHandler;
            }

            // Remove any existing handler and add the new one
            page.Disappearing -= disappearingHandler;
            page.Disappearing += disappearingHandler;
        }

        /// <summary>
        /// Safely navigates back if possible
        /// </summary>
        /// <param name="page">The page to navigate from</param>
        /// <returns>Task representing the navigation operation</returns>
        public static async Task SafeGoBackAsync(this Page page)
        {
            try
            {
                // Try to use navigation if available
                if (page.Navigation != null && page.Navigation.NavigationStack.Count > 1)
                {
                    await page.Navigation.PopAsync();
                    return;
                }

                // Try to use Shell navigation if available
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // If we can't go back, just do nothing
                System.Diagnostics.Debug.WriteLine("PageExtensions: Cannot navigate back - no navigation context available");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PageExtensions: Error navigating back: {ex.Message}");
            }
        }
    }
}