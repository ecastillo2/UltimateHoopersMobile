using Microsoft.Maui.Controls;
using System;
using Domain; // Import Domain namespace for AccountType enum

namespace UltimateHoopers.Pages
{
    public partial class FindRunsPage : ContentPage
    {
        private bool _isMapView = false;

        public FindRunsPage()
        {
            InitializeComponent();

            // Set default values for filters
            if (DatePicker.Items.Count > 0)
                DatePicker.SelectedIndex = 0;

            if (SkillLevelPicker.Items.Count > 0)
                SkillLevelPicker.SelectedIndex = 0;

            // Update UI based on user account type
            UpdateCreateRunButtonVisibility();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Make sure the UI is updated when the page appears
            UpdateCreateRunButtonVisibility();
        }

        // Update the Create Run button visibility based on account type
        private void UpdateCreateRunButtonVisibility()
        {
            try
            {
                // Check if user is authenticated and has a Host account
                bool isHost = App.User?.IsHost ?? false;

                Console.WriteLine($"User is host: {isHost}");

                // Find the Create Run button by name
                var createRunButton = this.FindByName<Button>("CreateRunButton");

                if (createRunButton != null)
                {
                    Console.WriteLine("Found CreateRunButton by name");

                    // Update button based on account type
                    if (!isHost)
                    {
                        createRunButton.Text = "Upgrade to Host";
                        // Remove existing event handlers to avoid duplicates
                        createRunButton.Clicked -= OnCreateRunClicked;
                        createRunButton.Clicked -= OnUpgradeAccountClicked;
                        // Add the upgrade event handler
                        createRunButton.Clicked += OnUpgradeAccountClicked;
                    }
                    else
                    {
                        createRunButton.Text = "Create Run";
                        // Remove existing event handlers to avoid duplicates
                        createRunButton.Clicked -= OnUpgradeAccountClicked;
                        createRunButton.Clicked -= OnCreateRunClicked;
                        // Add the create run event handler
                        createRunButton.Clicked += OnCreateRunClicked;
                    }
                }
                else
                {
                    Console.WriteLine("CreateRunButton not found by name - falling back to search");

                    // Fallback: search through the visual tree if button can't be found by name
                    foreach (var element in ((Grid)Content).Children)
                    {
                        if (element is ScrollView scrollView)
                        {
                            var stackLayout = scrollView.Content as VerticalStackLayout;
                            if (stackLayout != null)
                            {
                                // Find the Search Section frame
                                var searchFrame = stackLayout.Children.FirstOrDefault(c => c is Frame) as Frame;
                                if (searchFrame != null)
                                {
                                    var frameContent = searchFrame.Content as VerticalStackLayout;
                                    if (frameContent != null)
                                    {
                                        // Find the button grid (the last child in the frame content)
                                        var buttonGrid = frameContent.Children.LastOrDefault() as Grid;
                                        if (buttonGrid != null)
                                        {
                                            // Get the create run button (second column)
                                            var button = buttonGrid.Children.LastOrDefault() as Button;
                                            if (button != null)
                                            {
                                                Console.WriteLine("Found Create Run button through visual tree");

                                                // Update button based on account type
                                                if (!isHost)
                                                {
                                                    button.Text = "Upgrade to Host";
                                                    // Remove existing event handlers to avoid duplicates
                                                    button.Clicked -= OnCreateRunClicked;
                                                    button.Clicked -= OnUpgradeAccountClicked;
                                                    // Add the upgrade event handler
                                                    button.Clicked += OnUpgradeAccountClicked;
                                                }
                                                else
                                                {
                                                    button.Text = "Create Run";
                                                    // Remove existing event handlers to avoid duplicates
                                                    button.Clicked -= OnUpgradeAccountClicked;
                                                    button.Clicked -= OnCreateRunClicked;
                                                    // Add the create run event handler
                                                    button.Clicked += OnCreateRunClicked;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating create run button visibility: {ex.Message}");
            }
        }

        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//HomePage");
        }

        private async void OnStatsNavigationClicked(object sender, EventArgs e)
        {
            // Navigate to the Stats page
            await Shell.Current.GoToAsync("//StatsPage");
        }

        private async void OnGamesNavigationClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//FindRunsPage");
        }

        private async void OnProfileNavigationClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Profile", "Profile page coming soon!", "OK");
        }

        // Search and Filter handlers
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // Filter runs based on search text
            // This would call a method to update the displayed runs
            FilterRuns();
        }

        private void OnDateSelected(object sender, EventArgs e)
        {
            // Filter runs based on selected date
            FilterRuns();
        }

        private void OnSkillLevelSelected(object sender, EventArgs e)
        {
            // Filter runs based on selected skill level
            FilterRuns();
        }

        private void FilterRuns()
        {
            // This method would apply all current filters and update the UI
            // In a real implementation, this might query a database or API

            string searchText = SearchEntry.Text?.ToLower() ?? string.Empty;
            string dateFilter = DatePicker.SelectedItem?.ToString() ?? "All";
            string skillFilter = SkillLevelPicker.SelectedItem?.ToString() ?? "All Levels";

            // For demo purposes, just display what we're filtering by
            Console.WriteLine($"Filtering runs: Search='{searchText}', Date='{dateFilter}', Skill='{skillFilter}'");

            // In a real app, you would update the runs collection here
        }

        // Action methods
        private async void OnToggleMapViewClicked(object sender, EventArgs e)
        {
            _isMapView = !_isMapView;

            // Toggle between list and map view
            Button button = (Button)sender;
            button.Text = _isMapView ? "Show List View" : "Show Map View";

            // In a real app, you would show/hide different views here
            await DisplayAlert("View Toggle",
                _isMapView ? "Map view coming soon!" : "Showing list view",
                "OK");
        }

        private async void OnCreateRunClicked(object sender, EventArgs e)
        {
            // Double-check if user is authorized to create runs
            if (App.User?.IsHost != true)
            {
                await DisplayAlert(
                    "Host Account Required",
                    "You need a Host account to create runs. Host accounts cost $9.99/month. Would you like to upgrade your account?",
                    "OK");
                return;
            }

            // If we get here, user is a host and can create runs
            await DisplayAlert("Create Run", "Create run feature coming soon!", "OK");
        }

        private async void OnUpgradeAccountClicked(object sender, EventArgs e)
        {
            bool shouldUpgrade = await DisplayAlert(
                "Upgrade Account",
                "Upgrading to a Host account ($9.99/month) allows you to create and manage pickup games. Would you like to upgrade?",
                "Yes", "No");

            if (shouldUpgrade)
            {
                // Navigate to account upgrade page or show upgrade modal
                await DisplayAlert("Account Upgrade", "Account upgrade feature coming soon! Your card will be charged $9.99/month after completing the upgrade.", "OK");

                // In a real implementation, you would call an API to upgrade the account
                // For example:
                // await _accountService.UpgradeToHostAsync();

                // Then update the App.User object
                // App.User.IsHost = true;

                // Then refresh the UI
                // UpdateCreateRunButtonVisibility();
            }
        }

        private async void OnJoinGameClicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Join Game",
                "Would you like to join this pickup game?",
                "Yes", "No");

            if (result)
            {
                await DisplayAlert("Success", "You've joined the game!", "OK");
                // In a real app, you would send a join request to the API
                // and update the UI accordingly
            }
        }

        private async void OnLoadMoreClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Load More", "Loading more games...", "OK");
            // In a real app, you would load the next page of results
        }
    }
}