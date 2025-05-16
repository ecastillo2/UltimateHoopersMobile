using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using Microsoft.Extensions.DependencyInjection;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class HoopersPage : ContentPage
    {
        // ViewModel for data binding
        private ProfileViewModel _viewModel;

        public HoopersPage()
        {
            InitializeComponent();

            // Create and initialize the ViewModel
            _viewModel = new ProfileViewModel();
            BindingContext = _viewModel;

            // Load data when page appears
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.LoadProfilesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hoopers: {ex.Message}");
                await DisplayAlert("Error", "Could not load player data. Please try again later.", "OK");
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

        // Events from XAML
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            //_viewModel.FilterHoopers(e.NewTextValue);
        }

        private void OnFilterGuardsClicked(object sender, EventArgs e)
        {
            //_viewModel.FilterByPosition("Guard");
        }

        private void OnFilterForwardsClicked(object sender, EventArgs e)
        {
            //_viewModel.FilterByPosition("Forward");
        }

        private void OnFilterCentersClicked(object sender, EventArgs e)
        {
            //_viewModel.FilterByPosition("Center");
        }

        private void OnFilterNearbyClicked(object sender, EventArgs e)
        {
            //_viewModel.FilterByLocation("Atlanta");
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ProfileViewModel hooper)
            {
                //await DisplayAlert("Connect", $"Connecting with ");
            }
        }
    }
}