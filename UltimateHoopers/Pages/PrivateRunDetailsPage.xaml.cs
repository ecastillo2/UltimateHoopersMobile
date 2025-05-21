using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using UltimateHoopers.Models;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class PrivateRunDetailsPage : ContentPage
    {
        private PrivateRunDetailsViewModel _viewModel;

        // Default constructor (needed for design-time support)
        public PrivateRunDetailsPage()
        {
            InitializeComponent();

            // Create default ViewModel with an empty run
            _viewModel = new RunDetailsViewModel(new Run());
            BindingContext = _viewModel;
        }

        // Main constructor with a run parameter
        public PrivateRunDetailsPage(Run run)
        {
            InitializeComponent();

            // Create ViewModel with the provided run
            _viewModel = new RunDetailsViewModel(run);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh the run details when the page appears
            _viewModel.RefreshRunDetails();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("RunDetailsPage: OnHomeClicked - using navigation");

                await Shell.Current.GoToAsync("//HomePage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RunDetailsPage: Error navigating to HomePage: {ex.Message}");

                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again.",
                    "OK");
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
    }
}