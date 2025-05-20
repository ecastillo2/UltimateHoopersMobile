using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using UltimateHoopers.Pages;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Simple and direct navigation helper for the most common navigation scenarios
    /// </summary>
    public static class DirectNavigationHelper
    {
        /// <summary>
        /// Navigate directly to HomePage, bypassing complex navigation logic
        /// </summary>
        public static async Task GoToHomePageAsync()
        {
            try
            {
                Debug.WriteLine("DirectNavigationHelper: GoToHomePageAsync called");

                // Create the HomePage instance
                HomePage homePage = null;

                // Try DI first
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                homePage = serviceProvider.GetService<HomePage>();

                // Fallback to direct creation
                if (homePage == null)
                {
                    Debug.WriteLine("DirectNavigationHelper: Creating HomePage manually");
                    homePage = new HomePage();
                }

                // Execute on UI thread
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Check if we're in a Shell environment
                        if (Shell.Current != null)
                        {
                            try
                            {
                                Debug.WriteLine("DirectNavigationHelper: Using Shell.GoToAsync to HomePage");
                                await Shell.Current.GoToAsync("//HomePage");
                                return;
                            }
                            catch (Exception shellEx)
                            {
                                Debug.WriteLine($"DirectNavigationHelper: Shell navigation failed: {shellEx.Message}");
                            }
                        }

                        // Direct approach if Shell navigation fails or is unavailable
                        Debug.WriteLine("DirectNavigationHelper: Setting NavigationPage with HomePage directly");
                        Application.Current.MainPage = new NavigationPage(homePage);
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"DirectNavigationHelper: Navigation error: {navEx.Message}");

                        // Last resort
                        Application.Current.MainPage = homePage;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DirectNavigationHelper: Critical error: {ex.Message}");

                // Even if everything fails, try one last option on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Application.Current.MainPage = new HomePage();
                    }
                    catch
                    {
                        // Nothing more we can do
                    }
                });
            }
        }

        /// <summary>
        /// Navigate directly to PostsPage, bypassing complex navigation logic
        /// </summary>
        public static async Task GoToPostsPageAsync()
        {
            try
            {
                Debug.WriteLine("DirectNavigationHelper: GoToPostsPageAsync called");

                // Create the PostsPage instance
                PostsPage postsPage = null;

                // Try DI first
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                postsPage = serviceProvider.GetService<PostsPage>();

                // Fallback to direct creation
                if (postsPage == null)
                {
                    Debug.WriteLine("DirectNavigationHelper: Creating PostsPage manually");
                    postsPage = new PostsPage();
                }

                // Execute on UI thread
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Check if we're in a Shell environment
                        if (Shell.Current != null)
                        {
                            try
                            {
                                Debug.WriteLine("DirectNavigationHelper: Using Shell.GoToAsync to PostsPage");
                                await Shell.Current.GoToAsync("//PostsPage");
                                return;
                            }
                            catch (Exception shellEx)
                            {
                                Debug.WriteLine($"DirectNavigationHelper: Shell navigation failed: {shellEx.Message}");
                            }
                        }

                        // Direct approach if Shell navigation fails or is unavailable
                        Debug.WriteLine("DirectNavigationHelper: Setting NavigationPage with PostsPage directly");
                        Application.Current.MainPage = new NavigationPage(postsPage);
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"DirectNavigationHelper: Navigation error: {navEx.Message}");

                        // Last resort
                        Application.Current.MainPage = postsPage;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DirectNavigationHelper: Critical error: {ex.Message}");

                // Even if everything fails, try one last option on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Application.Current.MainPage = new PostsPage();
                    }
                    catch
                    {
                        // Nothing more we can do
                    }
                });
            }
        }

        /// <summary>
        /// Navigate directly to a specific page by name
        /// </summary>
        public static async Task GoToPageAsync(string pageName)
        {
            if (string.Equals(pageName, "HomePage", StringComparison.OrdinalIgnoreCase))
            {
                await GoToHomePageAsync();
            }
            else if (string.Equals(pageName, "PostsPage", StringComparison.OrdinalIgnoreCase))
            {
                await GoToPostsPageAsync();
            }
            else
            {
                Debug.WriteLine($"DirectNavigationHelper: Unknown page name: {pageName}");
            }
        }
    }
}