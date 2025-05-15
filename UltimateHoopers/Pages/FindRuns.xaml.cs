namespace UltimateHoopers.Pages
{
    using Microsoft.Maui.Controls;
    using System;

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
            await Shell.Current.GoToAsync("//StatsPage");
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
            await DisplayAlert("Create Run", "Create run feature coming soon!", "OK");
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