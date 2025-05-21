using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using UltimateHoopers.Helpers;
using UltimateHoopers.Models;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class FindRunsPage : ContentPage
    {
        private RunViewModel _viewModel;

        public FindRunsPage()
        {
            InitializeComponent();

            // Get ViewModel from binding context
            _viewModel = BindingContext as RunViewModel;

            // Additional setup that couldn't be done in XAML
            SetupUI();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh runs when page appears
            MainThread.BeginInvokeOnMainThread(async () => {
                try
                {
                    await _viewModel.RefreshRuns();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error refreshing runs: {ex.Message}");
                }
            });

            // Update UI based on user account type
            UpdateCreateRunButtonVisibility();
        }

        private void SetupUI()
        {
            // Set default picker values if not already set
            if (DatePicker.SelectedItem == null && DatePicker.Items.Count > 0)
            {
                DatePicker.SelectedIndex = 0;
            }

            if (SkillLevelPicker.SelectedItem == null && SkillLevelPicker.Items.Count > 0)
            {
                SkillLevelPicker.SelectedIndex = 0;
            }

            // Register boolean converter if needed
            if (!Resources.TryGetValue("InvertBoolConverter", out _))
            {
                Resources.Add("InvertBoolConverter", new InvertBoolConverter());
            }
        }

        // Update the Create Run button visibility based on account type
        private void UpdateCreateRunButtonVisibility()
        {
            try
            {
                // Check if user is authenticated and has a Host account
                bool isHost = App.User?.IsHost ?? false;

                // Find the Create Run button by name
                if (CreateRunButton != null)
                {
                    // Update button based on account type
                    if (!isHost)
                    {
                        CreateRunButton.Text = "Upgrade to Host";
                    }
                    else
                    {
                        CreateRunButton.Text = "Create Run";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating create run button visibility: {ex.Message}");
            }
        }

        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("FindRunsPage: OnHomeClicked - using DirectNavigationHelper");

                // Use the simpler, more direct navigation helper
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindRunsPage: Error navigating to HomePage: {ex.Message}");

                // Show an error message to the user
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnStatsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to the Stats page
            try
            {
                await Shell.Current.GoToAsync("//StatsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to StatsPage: {ex.Message}");
                await DisplayAlert("Navigation Error", "Could not navigate to stats page", "OK");
            }
        }

        private async void OnProfileNavigationClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Profile", "Profile page coming soon!", "OK");
        }
    }

    // Boolean converter for inverting boolean values in XAML
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }
}