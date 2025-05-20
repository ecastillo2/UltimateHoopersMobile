using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Pages;

namespace UltimateHoopers.Helpers
{
    /// <summary>
    /// Helper for navigating to the FAQ page from anywhere in the app
    /// </summary>
    public static class FAQNavigationHelper
    {
        /// <summary>
        /// Navigate to the FAQ page
        /// </summary>
        public static async Task GoToFAQPageAsync()
        {
            try
            {
                Debug.WriteLine("FAQNavigationHelper: GoToFAQPageAsync called");

                // Create the FAQ page instance
                FAQPage faqPage = null;

                // Try DI first
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                faqPage = serviceProvider.GetService<FAQPage>();

                // Fallback to direct creation
                if (faqPage == null)
                {
                    Debug.WriteLine("FAQNavigationHelper: Creating FAQPage manually");
                    faqPage = new FAQPage();
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
                                Debug.WriteLine("FAQNavigationHelper: Using Shell.GoToAsync to FAQPage");
                                await Shell.Current.GoToAsync("//FAQPage");
                                return;
                            }
                            catch (Exception shellEx)
                            {
                                Debug.WriteLine($"FAQNavigationHelper: Shell navigation failed: {shellEx.Message}");

                                // Try regular navigation if Shell navigation fails
                                if (Shell.Current.Navigation != null)
                                {
                                    await Shell.Current.Navigation.PushAsync(faqPage);
                                    return;
                                }
                            }
                        }

                        // Direct approach if Shell navigation fails or is unavailable
                        Debug.WriteLine("FAQNavigationHelper: Using direct navigation to FAQPage");

                        if (Application.Current.MainPage is NavigationPage navPage)
                        {
                            await navPage.PushAsync(faqPage);
                        }
                        else if (Application.Current.MainPage.Navigation != null)
                        {
                            await Application.Current.MainPage.Navigation.PushAsync(faqPage);
                        }
                        else
                        {
                            // Last resort: set as main page with navigation
                            Application.Current.MainPage = new NavigationPage(faqPage);
                        }
                    }
                    catch (Exception navEx)
                    {
                        Debug.WriteLine($"FAQNavigationHelper: Navigation error: {navEx.Message}");

                        // Last resort
                        try
                        {
                            Application.Current.MainPage = faqPage;
                        }
                        catch
                        {
                            // Nothing more we can do
                            Debug.WriteLine("FAQNavigationHelper: All navigation attempts failed");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FAQNavigationHelper: Critical error: {ex.Message}");

                // Even if everything fails, try one last option on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Application.Current.MainPage = new FAQPage();
                    }
                    catch
                    {
                        // Nothing more we can do
                    }
                });
            }
        }

        /// <summary>
        /// Register the FAQ page with the Shell for navigation
        /// </summary>
        public static void RegisterFAQPageWithShell()
        {
            try
            {
                // Register the FAQ page route
                if (!IsRouteRegistered("faqpage"))
                {
                    Routing.RegisterRoute("faqpage", typeof(FAQPage));
                    Debug.WriteLine("FAQNavigationHelper: Registered FAQ page route");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FAQNavigationHelper: Error registering FAQ page route: {ex.Message}");
            }
        }

        // Helper method to check if a route is already registered
        private static bool IsRouteRegistered(string route)
        {
            try
            {
                // Try to get the route handler
                var handler = Routing.GetOrCreateContent(route);
                return handler != null;
            }
            catch
            {
                // If an exception is thrown, the route is not registered
                return false;
            }
        }
    }
}