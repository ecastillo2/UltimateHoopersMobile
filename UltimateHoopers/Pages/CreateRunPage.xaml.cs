using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace UltimateHoopers.Pages
{
    public partial class CreateRunPage : ContentPage
    {
        // Properties for binding
        public DateTime TodayDate { get; private set; }
        private List<string> _invitedFriends = new List<string>();
        private string _selectedCourt = string.Empty;

        public CreateRunPage()
        {
            InitializeComponent();
            InitializePageData();
        }

        private void InitializePageData()
        {
            // Set date picker minimum to today
            TodayDate = DateTime.Today;
            BindingContext = this;

            // Set default values
            RunDatePicker.Date = DateTime.Today;
            StartTimePicker.Time = new TimeSpan(18, 0, 0); // 6:00 PM
            EndTimePicker.Time = new TimeSpan(20, 0, 0);   // 8:00 PM

            // Listen for player count picker changes
            PlayerCountPicker.SelectedIndexChanged += OnPlayerCountPickerChanged;

            // Set initial validation state
            ValidateForm();
        }

        #region Navigation
        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            Navigation.PopToRootAsync();
        }

        private void OnSquadNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to squads page
            // Implementation depends on your navigation structure
        }

        private void OnSettingsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to settings page
            // Implementation depends on your navigation structure
        }
        #endregion

        #region Event Handlers
        private async void OnFindCourtsClicked(object sender, EventArgs e)
        {
            // In a real app, you would implement a court selection page/popup
            // For this example, we'll use a simple action sheet
            string action = await DisplayActionSheet(
                "Select a Court",
                "Cancel",
                null,
                "Central Park Courts",
                "Downtown Rec Center",
                "Westside Community Center",
                "Lincoln High School",
                "Memorial Park Courts");

            if (action != "Cancel" && !string.IsNullOrEmpty(action))
            {
                _selectedCourt = action;
                SelectedCourtLabel.Text = action;
                SelectedCourtLabel.TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];

                // Show map preview
                MapPreviewFrame.IsVisible = true;

                // Update validation
                ValidateForm();
            }
        }

        private async void OnCustomLocationClicked(object sender, EventArgs e)
        {
            // Show dialog to enter custom location
            string location = await DisplayPromptAsync(
                "Custom Location",
                "Enter address or location name:",
                "OK",
                "Cancel",
                placeholder: "e.g. 123 Main St, City, State");

            if (!string.IsNullOrWhiteSpace(location))
            {
                _selectedCourt = location;
                SelectedCourtLabel.Text = location;
                SelectedCourtLabel.TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];

                // Show map preview
                MapPreviewFrame.IsVisible = true;

                // Update validation
                ValidateForm();
            }
        }

        private void OnPlayerCountPickerChanged(object sender, EventArgs e)
        {
            // Show custom player count fields if "Other" is selected
            CustomPlayerCountGrid.IsVisible = PlayerCountPicker.SelectedIndex == 5; // "Other" is at index 5

            // Set initial value based on previous selection
            if (PlayerCountPicker.SelectedIndex < 5)
            {
                // Extract number from option text (e.g., "3-on-3 (6 players)" -> 6)
                string selectedOption = (string)PlayerCountPicker.SelectedItem;
                int startIndex = selectedOption.IndexOf('(') + 1;
                int endIndex = selectedOption.IndexOf(' ', startIndex);
                if (int.TryParse(selectedOption.Substring(startIndex, endIndex - startIndex), out int playerCount))
                {
                    PlayerCountStepper.Value = playerCount;
                    CustomPlayerCountEntry.Text = playerCount.ToString();
                }
            }
        }

        private void OnPlayerCountStepperChanged(object sender, ValueChangedEventArgs e)
        {
            CustomPlayerCountEntry.Text = e.NewValue.ToString("0");
        }

        private void OnTimePickerChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Time")
            {
                // Ensure end time is after start time
                if (EndTimePicker.Time <= StartTimePicker.Time)
                {
                    // Set end time to 2 hours after start time
                    EndTimePicker.Time = StartTimePicker.Time.Add(new TimeSpan(2, 0, 0));
                }
            }
        }

        private void OnPublicSwitchToggled(object sender, ToggledEventArgs e)
        {
            // Additional logic if needed when toggling public/private
        }

        private async void OnInviteFriendsClicked(object sender, EventArgs e)
        {
            await ShowFriendSelectionDialog();
        }

        private void OnEditInvitesClicked(object sender, EventArgs e)
        {
            OnInviteFriendsClicked(sender, e);
        }

        private async void OnCreateRunClicked(object sender, EventArgs e)
        {
            // Validate form before proceeding
            if (!ValidateForm())
            {
                return;
            }

            // Show loading indicator
            bool confirmed = await DisplayAlert(
                "Create Run",
                "Are you ready to create this run and invite players?",
                "Create Run", "Cancel");

            if (confirmed)
            {
                // Simulate API call
                await SimulateLoadingAsync("Creating your run...");

                // Show success message
                await DisplayAlert(
                    "Success",
                    "Your run has been created! Players will be notified of the invitation.",
                    "OK");

                // Navigate back
                await Navigation.PopAsync();
            }
        }
        #endregion

        #region Helper Methods
        private async Task ShowFriendSelectionDialog()
        {
            // In a real app, you would implement a friend selection page
            // For this example, we'll use a simple action sheet with some sample friends
            List<string> selectedFriends = new List<string>(_invitedFriends);

            // Show a series of dialogs to select friends (simplified)
            bool selecting = true;
            while (selecting)
            {
                string[] friendOptions = {
                    "Michael Jordan",
                    "LeBron James",
                    "Stephen Curry",
                    "Kevin Durant",
                    "Giannis Antetokounmpo",
                    "James Harden",
                    "Luka Doncic"
                };

                // Filter out already selected friends
                var availableFriends = new List<string>();
                foreach (var friend in friendOptions)
                {
                    if (!selectedFriends.Contains(friend))
                    {
                        availableFriends.Add(friend);
                    }
                }

                string action;
                if (availableFriends.Count > 0)
                {
                    action = await DisplayActionSheet(
                        "Select Friends to Invite",
                        "Done",
                        null,
                        availableFriends.ToArray());
                }
                else
                {
                    await DisplayAlert("All Friends Invited", "You've invited all available friends!", "OK");
                    action = "Done";
                }

                if (action == "Done")
                {
                    selecting = false;
                }
                else if (!string.IsNullOrEmpty(action))
                {
                    selectedFriends.Add(action);
                }
            }

            // Save selected friends
            _invitedFriends = selectedFriends;

            // Update UI
            InvitedPlayersFrame.IsVisible = _invitedFriends.Count > 0;
            InvitedPlayersLabel.Text = $"{_invitedFriends.Count} player{(_invitedFriends.Count != 1 ? "s" : "")} invited";
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Check if court is selected
            if (string.IsNullOrEmpty(_selectedCourt))
            {
                isValid = false;
            }

            // Check if name is entered
            if (string.IsNullOrWhiteSpace(RunNameEntry.Text))
            {
                isValid = false;
            }

            // Check if custom player count is entered (when applicable)
            if (PlayerCountPicker.SelectedIndex == 5 && string.IsNullOrWhiteSpace(CustomPlayerCountEntry.Text))
            {
                isValid = false;
            }

            return isValid;
        }

        private async Task SimulateLoadingAsync(string message)
        {
            // Show loading indicator
            await DisplayAlert("Processing", message, "Please wait");

            // Simulate delay
            await Task.Delay(1000);
        }
        #endregion
    }
}