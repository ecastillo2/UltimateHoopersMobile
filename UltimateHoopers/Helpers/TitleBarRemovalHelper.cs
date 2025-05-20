using Microsoft.Maui.Controls;
using System;

namespace UltimateHoopers.Helpers
{
    public static class TitleBarRemovalHelper
    {
        /// <summary>
        /// Removes the title bar for all pages in the application
        /// </summary>
        public static void RemoveTitleBarForAllPages()
        {
            // Hook into page appearing events to ensure title bar is always hidden
            Application.Current.PageAppearing += (sender, e) =>
            {
                if (e is Page currentPage) // Changed variable name to avoid naming conflict
                {
                    RemoveTitleBar(currentPage);
                }
            };
        }

        /// <summary>
        /// Removes the title bar for a specific page
        /// </summary>
        public static void RemoveTitleBar(Page page)
        {
            if (page == null)
                return;

            // Clear the page title
            page.Title = string.Empty;

            // Hide navigation bar if the page is in a Shell
            Shell.SetNavBarIsVisible(page, false);

            // For iOS additional handling
#if IOS
            // Hide status bar (iOS specific)
            // This is handled in AppDelegate.cs
#endif

            // For Android additional handling
#if ANDROID
            // Hide status bar (Android specific)
            // This is handled in MainActivity.cs
#endif
        }

        /// <summary>
        /// Apply title bar removal to all pages in a navigation stack
        /// </summary>
        public static void ApplyToNavigationStack(INavigation navigation)
        {
            if (navigation?.NavigationStack == null)
                return;

            foreach (var page in navigation.NavigationStack)
            {
                RemoveTitleBar(page);
            }
        }

        /// <summary>
        /// Apply title bar removal to all items in a Shell
        /// </summary>
        public static void ApplyToShell(Shell shell)
        {
            if (shell == null)
                return;

            // Set the Shell's navigation bar visibility
            Shell.SetNavBarIsVisible(shell, false);

            // Process each ShellItem
            foreach (var shellItem in shell.Items)
            {
                if (shellItem is ShellItem item)
                {
                    foreach (var section in item.Items)
                    {
                        if (section is ShellSection shellSection)
                        {
                            foreach (var content in shellSection.Items)
                            {
                                if (content is ShellContent shellContent)
                                {
                                    // Get content page if already resolved
                                    if (shellContent.Content is Page contentPage)
                                    {
                                        RemoveTitleBar(contentPage);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Also apply to current page
            if (shell.CurrentPage != null)
            {
                RemoveTitleBar(shell.CurrentPage);
            }
        }
    }
}