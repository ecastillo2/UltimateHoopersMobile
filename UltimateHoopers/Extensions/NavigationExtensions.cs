using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Pages;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Extension methods for NavigationHelper to enhance stability and diagnostics
    /// </summary>
    public static class NavigationExtensions
    {
        /// <summary>
        /// Safely attempts to navigate to a specified route with multiple fallback mechanisms
        /// </summary>
        public static async Task SafeNavigateAsync(this Page page, string route)
        {
            try
            {
                Debug.WriteLine($"SafeNavigate: Starting navigation to {route}");

                // First check if we're already on the destination page
                if (IsAlreadyAtDestination(page, route))
                {
                    Debug.WriteLine($"SafeNavigate: Already at destination {route}, skipping navigation");
                    return;
                }

                // Try using NavigationHelper first
                try
                {
                    Debug.WriteLine($"SafeNavigate: Using NavigationHelper to navigate to {route}");
                    await NavigationHelper.NavigateTo(page, route);
                    return;
                }
                catch (Exception navHelperEx)
                {
                    Debug.WriteLine($"SafeNavigate: NavigationHelper failed: {navHelperEx.Message}");
                }

                // Try using Shell.GoToAsync if available
                if (Shell.Current != null)
                {
                    try
                    {
                        Debug.WriteLine($"SafeNavigate: Using Shell.GoToAsync to navigate to {route}");
                        await Shell.Current.GoToAsync(route);
                        return;
                    }
                    catch (Exception shellEx)
                    {
                        Debug.WriteLine($"SafeNavigate: Shell.GoToAsync failed: {shellEx.Message}");
                    }
                }

                // Try using page.Navigation if available
                if (page.Navigation != null)
                {
                    try
                    {
                        // Get page from route
                        var destinationPage = CreatePageFromRoute(route);
                        if (destinationPage != null)
                        {
                            Debug.WriteLine($"SafeNavigate: Using page.Navigation to navigate to new instance of {destinationPage.GetType().Name}");
                            await page.Navigation.PushAsync(destinationPage);
                            return;
                        }
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"SafeNavigate: Page.Navigation failed: {navEx.Message}");
                    }
                }

                // Final fallback - set Application.Current.MainPage
                try
                {
                    var destinationPage = CreatePageFromRoute(route);
                    if (destinationPage != null && Application.Current != null)
                    {
                        Debug.WriteLine($"SafeNavigate: Setting Application.Current.MainPage to {destinationPage.GetType().Name}");
                        Application.Current.MainPage = new NavigationPage(destinationPage);
                        return;
                    }
                }
                catch (Exception mainPageEx)
                {
                    Debug.WriteLine($"SafeNavigate: Setting MainPage failed: {mainPageEx.Message}");
                }

                Debug.WriteLine($"SafeNavigate: All navigation attempts failed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SafeNavigate: Unhandled exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the current page is already at the destination route
        /// </summary>
        private static bool IsAlreadyAtDestination(Page page, string route)
        {
            // Extract page name from route
            string pageNameFromRoute = route.Replace("//", "").Replace("/", "");

            // Get the current page type name
            string currentPageType = page.GetType().Name;

            // Strip "Page" suffix if present
            if (currentPageType.EndsWith("Page"))
            {
                currentPageType = currentPageType.Substring(0, currentPageType.Length - 4);
            }

            if (pageNameFromRoute.EndsWith("Page"))
            {
                pageNameFromRoute = pageNameFromRoute.Substring(0, pageNameFromRoute.Length - 4);
            }

            return string.Equals(currentPageType, pageNameFromRoute, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a page instance from a route
        /// </summary>
        private static Page CreatePageFromRoute(string route)
        {
            string pageName = route.Replace("//", "").Replace("/", "");

            // Try to get the page from DI first
            var serviceProvider = MauiProgram.CreateMauiApp().Services;

            switch (pageName.ToLowerInvariant())
            {
                case "homepage":
                    return serviceProvider.GetService<HomePage>() ?? new HomePage();

                case "postspage":
                    return serviceProvider.GetService<PostsPage>() ?? new PostsPage();

                case "hooperspage":
                    return serviceProvider.GetService<HoopersPage>() ?? new HoopersPage();

                case "statspage":
                    return serviceProvider.GetService<StatsPage>() ?? new StatsPage();

                case "findrunspage":
                    return serviceProvider.GetService<FindRunsPage>() ?? new FindRunsPage();

                // Add other page types as needed

                default:
                    return new HomePage(); // Default fallback
            }
        }
    }
}