using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper for handling auto-login related navigation issues
    /// </summary>
    public static class AutoLoginNavigationHelper
    {
        private static bool _isShellInitialized = false;
        private static bool _isInitialNavigationComplete = false;

        /// <summary>
        /// Indicates if initial shell navigation is complete
        /// </summary>
        public static bool IsInitialNavigationComplete
        {
            get => _isInitialNavigationComplete;
            set => _isInitialNavigationComplete = value;
        }

        /// <summary>
        /// Initializes the Shell navigation context properly after auto-login
        /// </summary>
        public static async Task EnsureShellInitializedAsync()
        {
            if (_isShellInitialized)
                return;

            try
            {
                // Check if Shell is already initialized
                if (Shell.Current == null)
                {
                    Debug.WriteLine("AutoLoginNavigationHelper: Shell.Current is null, attempting to initialize");

                    // Try to get AppShell from DI
                    var serviceProvider = MauiProgram.CreateMauiApp().Services;
                    var authService = serviceProvider.GetService<Services.IAuthService>();
                    var appShell = serviceProvider.GetService<AppShell>() ?? new AppShell(authService);

                    // Set the main page to AppShell
                    if (Application.Current != null)
                    {
                        Debug.WriteLine("AutoLoginNavigationHelper: Setting Application.Current.MainPage to AppShell");
                        Application.Current.MainPage = appShell;

                        // Give time for shell to initialize
                        await Task.Delay(300);
                    }
                }

                // Verify Shell is now initialized
                if (Shell.Current != null)
                {
                    Debug.WriteLine("AutoLoginNavigationHelper: Shell is now initialized");
                    _isShellInitialized = true;

                    // Ensure initial navigation is complete by navigating to a default page
                    if (!_isInitialNavigationComplete)
                    {
                        // Try to navigate to default page to complete initialization
                        try
                        {
                            Debug.WriteLine("AutoLoginNavigationHelper: Completing initial navigation to ensure routes work");
                            await Shell.Current.GoToAsync("//PostsPage");
                            _isInitialNavigationComplete = true;
                        }
                        catch (Exception navEx)
                        {
                            Debug.WriteLine($"AutoLoginNavigationHelper: Initial navigation error: {navEx.Message}");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("AutoLoginNavigationHelper: Shell could not be initialized");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AutoLoginNavigationHelper: Shell initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Safe navigation method specifically for post-login navigation issues
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="route">Destination route</param>
        /// <returns>Task representing the navigation operation</returns>
        public static async Task SafePostLoginNavigateAsync(Page page, string route)
        {
            try
            {
                // First ensure shell is properly initialized
                await EnsureShellInitializedAsync();

                // Check if we're already at destination
                string targetPageName = route.Replace("//", "").Replace("/", "");

                if (Shell.Current?.CurrentPage?.GetType().Name.Equals(targetPageName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    Debug.WriteLine($"AutoLoginNavigationHelper: Already at {targetPageName}");
                    return;
                }

                // Try multiple navigation approaches
                Exception lastException = null;

                // Approach 1: Try Shell.GoToAsync
                if (Shell.Current != null)
                {
                    try
                    {
                        Debug.WriteLine($"AutoLoginNavigationHelper: Navigating via Shell.GoToAsync to {route}");
                        await Shell.Current.GoToAsync(route);
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        Debug.WriteLine($"AutoLoginNavigationHelper: Shell.GoToAsync failed: {ex.Message}");
                    }
                }

                // Approach 2: Try regular Page.Navigation
                if (page?.Navigation != null)
                {
                    try
                    {
                        var targetPage = CreatePage(targetPageName);
                        if (targetPage != null)
                        {
                            Debug.WriteLine($"AutoLoginNavigationHelper: Navigating via Page.Navigation.PushAsync");
                            await page.Navigation.PushAsync(targetPage);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        Debug.WriteLine($"AutoLoginNavigationHelper: Page.Navigation.PushAsync failed: {ex.Message}");
                    }
                }

                // Approach 3: Set MainPage directly if all else fails
                try
                {
                    var targetPage = CreatePage(targetPageName);
                    if (targetPage != null && Application.Current != null)
                    {
                        Debug.WriteLine($"AutoLoginNavigationHelper: Setting MainPage directly");
                        Application.Current.MainPage = new NavigationPage(targetPage);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Debug.WriteLine($"AutoLoginNavigationHelper: Setting MainPage failed: {ex.Message}");
                }

                // If we get here, all navigation approaches failed
                if (lastException != null)
                {
                    Debug.WriteLine($"AutoLoginNavigationHelper: All navigation methods failed. Last error: {lastException.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AutoLoginNavigationHelper: Unhandled exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Create page instance from name
        /// </summary>
        private static Page CreatePage(string pageName)
        {
            var serviceProvider = MauiProgram.CreateMauiApp().Services;

            switch (pageName.ToLowerInvariant())
            {
                case "homepage":
                    return serviceProvider.GetService<Pages.HomePage>() ?? new Pages.HomePage();

                case "postspage":
                    return serviceProvider.GetService<Pages.PostsPage>() ?? new Pages.PostsPage();

                case "statspage":
                    return serviceProvider.GetService<Pages.StatsPage>() ?? new Pages.StatsPage();

                default:
                    return new Pages.HomePage();
            }
        }
    }
}
