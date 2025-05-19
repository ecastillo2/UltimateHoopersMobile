using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UltimateHoopers.ViewModels;
using UltimateHoopers.Helpers;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class NotificationsPage : ContentPage
    {
        private readonly NotificationsViewModel _viewModel;
        private string _currentTab = "All";

        public NotificationsPage()
        {
            InitializeComponent();

            // Ensure navigation bar is hidden
            Shell.SetNavBarIsVisible(this, false);

            // Initialize ViewModel
            _viewModel = new NotificationsViewModel();
            BindingContext = _viewModel;

            // Load notifications when page appears
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.LoadNotificationsAsync(_currentTab);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
                await DisplayAlert("Error", "Could not load notifications. Please try again later.", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Configure the page when it appears
            ConfigurePageOnAppearing();
        }

        private void ConfigurePageOnAppearing()
        {
            // Ensure NavBar is hidden
            Shell.SetNavBarIsVisible(this, false);

            // Update tab selection to reflect current filter
            UpdateTabSelection();
        }

        #region Tab Selection
        private async void OnAllTabSelected(object sender, EventArgs e)
        {
            if (_currentTab == "All")
                return;

            _currentTab = "All";
            UpdateTabSelection();
            await _viewModel.LoadNotificationsAsync(_currentTab);
        }

        private async void OnGamesTabSelected(object sender, EventArgs e)
        {
            if (_currentTab == "Games")
                return;

            _currentTab = "Games";
            UpdateTabSelection();
            await _viewModel.LoadNotificationsAsync(_currentTab);
        }

        private async void OnActivityTabSelected(object sender, EventArgs e)
        {
            if (_currentTab == "Activity")
                return;

            _currentTab = "Activity";
            UpdateTabSelection();
            await _viewModel.LoadNotificationsAsync(_currentTab);
        }

        private void UpdateTabSelection()
        {
            // Reset all tabs
            AllTabFrame.BackgroundColor = Colors.Transparent;
            GamesTabFrame.BackgroundColor = Colors.Transparent;
            ActivityTabFrame.BackgroundColor = Colors.Transparent;

            // Reset all labels
            (AllTabFrame.Content as Label).TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];
            (GamesTabFrame.Content as Label).TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];
            (ActivityTabFrame.Content as Label).TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];

            // Set selected tab
            switch (_currentTab)
            {
                case "All":
                    AllTabFrame.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
                    (AllTabFrame.Content as Label).TextColor = Colors.White;
                    break;
                case "Games":
                    GamesTabFrame.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
                    (GamesTabFrame.Content as Label).TextColor = Colors.White;
                    break;
                case "Activity":
                    ActivityTabFrame.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
                    (ActivityTabFrame.Content as Label).TextColor = Colors.White;
                    break;
            }
        }
        #endregion

        #region Navigation
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

        private async void OnGamesNavigationClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//FindRunsPage");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Navigate to notification settings page
            await Navigation.PushAsync(new NotificationSettingsPage());
        }

        private async void OnMarkAllAsReadClicked(object sender, EventArgs e)
        {
            try
            {
                // Ask for confirmation
                bool confirm = await DisplayAlert("Mark All as Read",
                    "Would you like to mark all notifications as read?",
                    "Yes", "No");

                if (confirm)
                {
                    // Use the ViewModel's command to mark all as read
                    _viewModel.MarkAllAsReadCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all as read: {ex.Message}");
                await DisplayAlert("Error", "Could not mark notifications as read", "OK");
            }
        }
        #endregion
    }
}