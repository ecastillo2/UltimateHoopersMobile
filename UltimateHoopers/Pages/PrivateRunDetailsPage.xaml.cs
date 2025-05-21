using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using UltimateHoopers.Models;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class PrivateRunDetailsPage : ContentPage
    {
        private PrivateRunDetailsViewModel _viewModel;

        public PrivateRunDetailsPage(Run run)
        {
            InitializeComponent();

            // Create the view model with the run parameter
            _viewModel = new PrivateRunDetailsViewModel(run);
            BindingContext = _viewModel;
        }

        // Parameterless constructor for XAML preview and DI
        public PrivateRunDetailsPage()
        {
            InitializeComponent();

            // Create a default run if no parameter is provided
            var defaultRun = new Run
            {
                Id = "default",
                Name = "Sample Run",
                Location = "Sample Location",
                Address = "123 Sample St",
                Date = DateTime.Now.AddDays(1),
                Time = "6:00 PM - 8:00 PM",
                HostName = "Sample Host",
                SkillLevel = "All Levels",
                GameType = "5-on-5",
                IsPublic = true,
                Description = "Sample run description",
                PlayerLimit = 10,
                CurrentPlayerCount = 5,
                Cost = 0,
                Distance = 1.5
            };

            _viewModel = new PrivateRunDetailsViewModel(defaultRun);
            BindingContext = _viewModel;
        }

        private async void OnBackClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating back: {ex.Message}");
            }
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//HomePage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to home: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to home page", "OK");
            }
        }

        private async void OnSquadNavigationClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Squad", "Squad page coming soon!", "OK");
        }

        private async void OnSettingsNavigationClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Settings", "Settings page coming soon!", "OK");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh run details when page appears
            _viewModel?.RefreshRunDetails();
        }
    }
}