using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Pages;
using UltimateHoopers.Services;

namespace UltimateHoopers.Extensions
{
    /// <summary>
    /// Extension methods for the LoginPage to add the ability to navigate to Shell with hamburger menu
    /// </summary>
    public static class LoginPageExtensions
    {
        /// <summary>
        /// Navigates to the AppShell after successful login
        /// </summary>
        /// <param name="loginPage">The login page</param>
        /// <param name="authService">The authentication service</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task NavigateToShellAfterLoginAsync(this LoginPage loginPage, IAuthService authService)
        {
            try
            {
                Debug.WriteLine("LoginPageExtensions: NavigateToShellAfterLoginAsync called");

                // Use the LoginNavigationHelper to handle the navigation
                await Helpers.LoginNavigationHelper.NavigateToShellAfterLoginAsync(authService);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginPageExtensions: Error navigating to Shell: {ex.Message}");

                // Show an error message to the user
                await loginPage.DisplayAlert(
                    "Navigation Error",
                    "There was a problem navigating to the main app. Please try again.",
                    "OK");

                // In case of error, we can try a fallback navigation approach
                try
                {
                    Debug.WriteLine("LoginPageExtensions: Trying fallback navigation");

                    // Create a new AppShell directly
                    var appShell = new AppShell(authService);

                    // Ensure FlyoutBehavior is set to show the hamburger menu
                    appShell.FlyoutBehavior = FlyoutBehavior.Flyout;

                    // Set the main page to the shell
                    Application.Current.MainPage = appShell;

                    Debug.WriteLine("LoginPageExtensions: Fallback navigation successful");
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"LoginPageExtensions: Fallback navigation failed: {fallbackEx.Message}");

                    // If all else fails, we can't do much more
                    await loginPage.DisplayAlert(
                        "Critical Error",
                        "Could not navigate to the main app. The app will now restart.",
                        "OK");

                    // Just create a new login page to effectively restart the login flow
                    Application.Current.MainPage = new LoginPage();
                }
            }
        }
    }
}