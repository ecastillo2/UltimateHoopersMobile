using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using UltimateHoopers.Converter;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class CreateRunPage : ContentPage
    {
        private CreateRunViewModel _viewModel;

        public CreateRunPage()
        {
            InitializeComponent();

            // Get the ViewModel from binding context
            _viewModel = BindingContext as CreateRunViewModel;

            // Setup UI that couldn't be done in XAML
            SetupUI();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Make sure only host accounts can access this page
            CheckHostAccountAccess();
        }

        private void SetupUI()
        {
            // Register boolean converter if needed
            if (!Resources.TryGetValue("InvertBoolConverter", out _))
            {
                Resources.Add("InvertBoolConverter", new InvertBoolConverter());
            }
        }

        private async void CheckHostAccountAccess()
        {
            try
            {
                bool isHost = App.User?.IsHost ?? false;

                if (!isHost)
                {
                    bool upgrade = await DisplayAlert(
                        "Host Account Required",
                        "You need a Host account to create runs. Host accounts cost $9.99/month. Would you like to upgrade your account?",
                        "Upgrade", "Cancel");

                    if (upgrade)
                    {
                        // In a real app, navigate to upgrade screen
                        await DisplayAlert("Account Upgrade", "Account upgrade feature coming soon!", "OK");
                    }

                    // Either way, go back since they can't access this page
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking host access: {ex.Message}");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Check for unsaved changes
            if (_viewModel.HasUnsavedChanges)
            {
                bool discard = await DisplayAlert(
                    "Unsaved Changes",
                    "You have unsaved changes. Are you sure you want to discard them?",
                    "Discard", "Keep Editing");

                if (!discard)
                {
                    return;
                }
            }

            await Navigation.PopAsync();
        }
    }

  
}