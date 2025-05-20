using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Pages;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper class for handling navigation across the app that works regardless of context
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        /// Navigates to the specified route from any context
        /// </summary>
        /// <param name="fromPage">The current page requesting navigation</param>
        /// <param name="route">The route to navigate to (Shell route format like "//HomePage")</param>
        public static async Task NavigateTo(Page fromPage, string route)
        {
            if (fromPage == null)
            {
                Debug.WriteLine("NavigationHelper: fromPage is null");
                NavigateWithMainPage(route);
                return;
            }

            try
            {
                // Check if already on the target page to avoid redundant navigation
                bool isAlreadyOnPage = IsAlreadyOnTargetPage(fromPage, route);
                if (isAlreadyOnPage)
                {
                    Debug.WriteLine($"NavigationHelper: Already on {route}, skipping navigation");
                    return;
                }

                // Try Shell navigation first if it's available
                if (Shell.Current != null)
                {
                    Debug.WriteLine($"NavigationHelper: Using Shell navigation to {route}");
                    await Shell.Current.GoToAsync(route);
                    return;
                }

                // If Shell isn't available, try regular navigation
                if (fromPage.Navigation != null)
                {
                    Debug.WriteLine($"NavigationHelper: Using Page navigation to {route}");
                    Page destinationPage = GetPageFromRoute(route);
                    await fromPage.Navigation.PushAsync(destinationPage);
                    return;
                }

                // Last resort: use MainPage directly
                Debug.WriteLine($"NavigationHelper: Using MainPage for {route}");
                NavigateWithMainPage(route);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error navigating to {route}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Try fallback options
                try
                {
                    if (fromPage.Navigation != null)
                    {
                        Page destinationPage = GetPageFromRoute(route);
                        await fromPage.Navigation.PushAsync(destinationPage);
                        return;
                    }

                    NavigateWithMainPage(route);
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"NavigationHelper: Fallback navigation failed: {fallbackEx.Message}");
                }
            }
        }

        private static bool IsAlreadyOnTargetPage(Page currentPage, string targetRoute)
        {
            // Clean up the route (remove // or / prefixes)
            string pageName = targetRoute.Replace("//", "").Replace("/", "");

            // Edge case: empty route
            if (string.IsNullOrEmpty(pageName))
            {
                return false;
            }

            // Check if current page is the same type as target
            string currentPageName = currentPage.GetType().Name;

            // Handle special cases with page names
            if (pageName.Equals("HomePage", StringComparison.OrdinalIgnoreCase) &&
                currentPageName.Equals("HomePage", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (pageName.Equals("PostsPage", StringComparison.OrdinalIgnoreCase) &&
                currentPageName.Equals("PostsPage", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // General case
            return currentPageName.Equals(pageName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns to the previous page
        /// </summary>
        /// <param name="fromPage">The current page</param>
        public static async Task GoBack(Page fromPage)
        {
            try
            {
                // Try Shell navigation first
                if (Shell.Current != null)
                {
                    Debug.WriteLine("NavigationHelper: Using Shell navigation to go back");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // If Shell isn't available, try regular navigation
                if (fromPage?.Navigation != null && fromPage.Navigation.NavigationStack.Count > 1)
                {
                    Debug.WriteLine("NavigationHelper: Using Page navigation to go back");
                    await fromPage.Navigation.PopAsync();
                    return;
                }

                // No navigation context available - we can't go back
                Debug.WriteLine("NavigationHelper: Can't go back - no navigation context available");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error going back: {ex.Message}");
            }
        }

        /// <summary>
        /// Presents a page as a modal
        /// </summary>
        /// <param name="fromPage">The current page</param>
        /// <param name="pageName">The name of the page to show modally</param>
        public static async Task ShowModal(Page fromPage, string pageName)
        {
            try
            {
                Page modalPage = GetPageFromRoute(pageName);

                if (fromPage?.Navigation != null)
                {
                    Debug.WriteLine($"NavigationHelper: Showing modal {pageName}");
                    await fromPage.Navigation.PushModalAsync(modalPage);
                    return;
                }

                if (Shell.Current?.Navigation != null)
                {
                    Debug.WriteLine($"NavigationHelper: Showing modal via Shell {pageName}");
                    await Shell.Current.Navigation.PushModalAsync(modalPage);
                    return;
                }

                if (Application.Current?.MainPage?.Navigation != null)
                {
                    Debug.WriteLine($"NavigationHelper: Showing modal via MainPage {pageName}");
                    await Application.Current.MainPage.Navigation.PushModalAsync(modalPage);
                    return;
                }

                Debug.WriteLine($"NavigationHelper: No navigation context available for modal {pageName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error showing modal: {ex.Message}");
            }
        }

        /// <summary>
        /// Dismisses a modal page
        /// </summary>
        /// <param name="fromPage">The current modal page to dismiss</param>
        public static async Task DismissModal(Page fromPage)
        {
            try
            {
                if (fromPage?.Navigation != null)
                {
                    Debug.WriteLine("NavigationHelper: Dismissing modal");
                    await fromPage.Navigation.PopModalAsync();
                    return;
                }

                if (Shell.Current?.Navigation != null)
                {
                    Debug.WriteLine("NavigationHelper: Dismissing modal via Shell");
                    await Shell.Current.Navigation.PopModalAsync();
                    return;
                }

                if (Application.Current?.MainPage?.Navigation != null)
                {
                    Debug.WriteLine("NavigationHelper: Dismissing modal via MainPage");
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                    return;
                }

                Debug.WriteLine("NavigationHelper: No navigation context available to dismiss modal");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error dismissing modal: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the root page of the application to a shell if not already using one
        /// </summary>
        public static void SetRootToShell()
        {
            try
            {
                if (Application.Current?.MainPage is not AppShell)
                {
                    Debug.WriteLine("NavigationHelper: Setting MainPage to new AppShell");

                    // Get service provider to create shell
                    var serviceProvider = MauiProgram.CreateMauiApp().Services;
                    var authService = serviceProvider?.GetService<Services.IAuthService>();

                    // Create shell with auth service if available
                    AppShell shell = serviceProvider?.GetService<AppShell>() ??
                                     (authService != null ? new AppShell(authService) : new AppShell());

                    // Set main page to shell
                    Application.Current.MainPage = shell;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error setting root to shell: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs out the user and returns to the login page
        /// </summary>
        public static async Task Logout()
        {
            try
            {
                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                var authService = serviceProvider?.GetService<Services.IAuthService>();

                // Logout using auth service if available
                if (authService != null)
                {
                    await authService.LogoutAsync();
                }
                else
                {
                    // Manual logout
                    App.AuthToken = null;
                    await SecureStorage.Default.SetAsync("auth_token", string.Empty);
                    await SecureStorage.Default.SetAsync("user_id", string.Empty);
                }

                // Create login page
                var loginPage = serviceProvider?.GetService<LoginPage>() ??
                               (authService != null ? new LoginPage(authService) : new LoginPage());

                // Set main page to login
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current.MainPage = loginPage;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error during logout: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates by setting the MainPage property
        /// </summary>
        private static void NavigateWithMainPage(string route)
        {
            if (Application.Current == null)
            {
                Debug.WriteLine("NavigationHelper: Application.Current is null");
                return;
            }

            Page page = GetPageFromRoute(route);

            try
            {
                // If we're navigating to a standard page and not inside a Shell,
                // we should wrap it in a NavigationPage to enable navigation
                if (!(page is AppShell) && !(page is NavigationPage) && Application.Current.MainPage is not AppShell)
                {
                    Debug.WriteLine($"NavigationHelper: Setting MainPage to NavigationPage with {route}");
                    Application.Current.MainPage = new NavigationPage(page);
                }
                else
                {
                    Debug.WriteLine($"NavigationHelper: Setting MainPage directly to {route}");
                    Application.Current.MainPage = page;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationHelper: Error in NavigateWithMainPage: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a page instance from a route
        /// </summary>
        private static Page GetPageFromRoute(string route)
        {
            // Clean up the route (remove // or / prefixes)
            string pageName = route.Replace("//", "").Replace("/", "");

            // Handle empty routes
            if (string.IsNullOrEmpty(pageName))
            {
                return new HomePage(); // Default to home
            }

            Debug.WriteLine($"NavigationHelper: Creating page for route {pageName}");

            // Create the appropriate page based on route
            switch (pageName.ToLowerInvariant())
            {
                case "homepage":
                    return new HomePage();
                case "postspage":
                    return new PostsPage();
                case "hooperspage":
                    return new HoopersPage();
                case "statspage":
                    return new StatsPage();
                case "findrunspage":
                    return new FindRunsPage();
                case "shoppage":
                    return new ShopPage();
                case "editprofilepage":
                    return new EditProfilePage();
                case "loginpage":
                    return new LoginPage();
                case "createaccountpage":
                    return new CreateAccountPage();
                case "playerprofilepage":
                    return new PlayerProfilePage(); // Note: This may need parameters
                case "videoplayerpage":
                    return new VideoPlayerPage(); // Note: This may need parameters
                case "notificationspage":
                    return new NotificationsPage();
                case "notificationsettingspage":
                    return new NotificationSettingsPage();
                // Add cases for any other pages in your app
                default:
                    Debug.WriteLine($"NavigationHelper: Unknown route {pageName}, defaulting to HomePage");
                    return new HomePage(); // Default to home
            }
        }
    }
}