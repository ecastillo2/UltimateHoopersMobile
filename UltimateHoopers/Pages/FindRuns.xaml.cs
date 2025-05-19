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
            // Check if user is authenticated and has a Host account
            bool isHost = App.User?.IsHost ?? false;

            // Find the Create Run button in the UI
            foreach (var element in ((Grid)Content).Children)
            {
                if (element is ScrollView scrollView)
                {
                    foreach (var childElement in ((VerticalStackLayout)scrollView.Content).Children)
                    {
                        if (childElement is Frame frame)
                        {
                            var createRunButton = frame.FindByName<Button>("CreateRunButton");
                            if (createRunButton != null)
                            {
                                // Show the button only for Host accounts
                                createRunButton.IsVisible = isHost;

                                // Update button text to reflect permissions
                                if (!isHost)
                                {
                                    createRunButton.Text = "Upgrade to Host";
                                    createRunButton.Clicked -= OnCreateRunClicked;
                                    createRunButton.Clicked += OnUpgradeAccountClicked;
                                }
                                else
                                {
                                    createRunButton.Text = "Create Run";
                                    createRunButton.Clicked -= OnUpgradeAccountClicked;
                                    createRunButton.Clicked += OnCreateRunClicked;
                                }
                            }
                        }
                    }
                }
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
            // Check if user is authorized to create runs
            if (App.User?.IsHost != true)
            {
                await DisplayAlert(
                    "Host Account Required",
                    "You need a Host account to create runs. Host accounts cost $9.99/month. Would you like to upgrade your account?",
                    "OK");
                return;
            }

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