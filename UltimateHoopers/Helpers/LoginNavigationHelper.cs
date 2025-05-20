using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using UltimateHoopers.Helpers;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper for handling navigation after login
    /// </summary>
    public static class LoginNavigationHelper
    {
        /// <summary>
        /// Navigates to the Shell after successful login, ensuring the hamburger menu is enabled
        /// </summary>
        /// <param name="authService">The authentication service to use for getting user info</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task NavigateToShellAfterLoginAsync(IAuthService authService)
        {
            try
            {
                Debug.WriteLine("LoginNavigationHelper: NavigateToShellAfterLoginAsync called");

                // Get service provider
                var serviceProvider = MauiProgram.CreateMauiApp().Services;

                // Create AppShell
                AppShell appShell = null;

                // Try to get AppShell from DI
                appShell = serviceProvider.GetService<AppShell>();
                Debug.WriteLine($"LoginNavigationHelper: AppShell obtained from DI: {(appShell != null ? "Yes" : "No")}");

                if (appShell == null)
                {
                    // Create new shell if DI failed
                    appShell = new AppShell(authService);
                    Debug.WriteLine("LoginNavigationHelper: Created new AppShell with auth service");
                }

                // Ensure FlyoutBehavior is set to show the hamburger menu
                appShell.FlyoutBehavior = FlyoutBehavior.Flyout;
                Debug.WriteLine($"LoginNavigationHelper: Set FlyoutBehavior to {appShell.FlyoutBehavior}");

                // Set the main page to the shell
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        Application.Current.MainPage = appShell;
                        Debug.WriteLine("LoginNavigationHelper: Set MainPage to AppShell");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LoginNavigationHelper: Error setting MainPage: {ex.Message}");
                        throw; // Re-throw to be caught by outer try-catch
                    }
                });

                // Give shell time to initialize
                await Task.Delay(500);

                // Use the helper to ensure Shell is fully initialized
                await AutoLoginNavigationHelper.EnsureShellInitializedAsync();

                // Update the welcome message in the Flyout header if user data is available
                if (App.User != null)
                {
                    string username = !string.IsNullOrWhiteSpace(App.User.UserName) ?
                        App.User.UserName :
                        (!string.IsNullOrWhiteSpace(App.User.FirstName) ? App.User.FirstName : "Player");

                    FlyoutHelper.UpdateFlyoutHeader(username);
                }

                // Ensure hamburger menu is enabled globally
                FlyoutHelper.EnableFlyoutMenuGlobally();

                // Mark initial navigation as complete
                AutoLoginNavigationHelper.IsInitialNavigationComplete = true;

                Debug.WriteLine("LoginNavigationHelper: Navigation to Shell complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginNavigationHelper: Error navigating to Shell: {ex.Message}");

                // Re-throw the exception to be handled by the caller
                throw;
            }
        }
    }
}