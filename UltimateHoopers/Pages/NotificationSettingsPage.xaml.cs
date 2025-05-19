using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace UltimateHoopers.Pages
{
    public partial class NotificationSettingsPage : ContentPage
    {
        // Create a local settings class to avoid any ambiguity
        private class LocalNotificationSettings
        {
            public bool EnablePushNotifications { get; set; } = true;
            public bool EnableEmailNotifications { get; set; } = false;
            public bool GameInvitations { get; set; } = true;
            public bool GameReminders { get; set; } = true;
            public bool FriendRequests { get; set; } = true;
            public bool PostInteractions { get; set; } = true;
            public bool SystemUpdates { get; set; } = false;
            public bool QuietHoursEnabled { get; set; } = false;
            public string QuietHoursStart { get; set; } = "22:00";
            public string QuietHoursEnd { get; set; } = "08:00";
        }

        // Store settings in local class
        private readonly LocalNotificationSettings _settings;

        public NotificationSettingsPage()
        {
            InitializeComponent();

            // Initialize settings with default values
            _settings = new LocalNotificationSettings();

            // Load settings
            LoadSettings();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Initialize time picker values
            if (TimeSpan.TryParse(_settings.QuietHoursStart, out TimeSpan startTime))
            {
                StartTimePicker.Time = startTime;
            }

            if (TimeSpan.TryParse(_settings.QuietHoursEnd, out TimeSpan endTime))
            {
                EndTimePicker.Time = endTime;
            }

            // Update UI based on settings
            UpdateQuietHoursFrameVisibility();
        }

        private async void LoadSettings()
        {
            try
            {
                // In a real app, you would load settings from a service or storage
                // For now, we're using the default values set in the constructor

                // You could implement something like:
                // var serviceProvider = MauiProgram.CreateMauiApp().Services;
                // var notificationService = serviceProvider.GetService<INotificationService>();
                // var serviceSettings = await notificationService.GetNotificationSettingsAsync();
                // _settings.EnablePushNotifications = serviceSettings.EnablePushNotifications;
                // ... and so on for other properties

                // For this demo, we'll just use some sample settings
                _settings.EnablePushNotifications = true;
                _settings.EnableEmailNotifications = false;
                _settings.GameInvitations = true;
                _settings.GameReminders = true;
                _settings.FriendRequests = true;
                _settings.PostInteractions = true;
                _settings.SystemUpdates = false;
                _settings.QuietHoursEnabled = false;
                _settings.QuietHoursStart = "22:00";
                _settings.QuietHoursEnd = "08:00";

                // Update UI
                UpdateUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notification settings: {ex.Message}");
                await DisplayAlert("Error", "Could not load notification settings", "OK");
            }
        }

        private void UpdateUI()
        {
            // Update switches
            PushNotificationsSwitch.IsToggled = _settings.EnablePushNotifications;
            EmailNotificationsSwitch.IsToggled = _settings.EnableEmailNotifications;
            QuietHoursSwitch.IsToggled = _settings.QuietHoursEnabled;
            GameInvitationsSwitch.IsToggled = _settings.GameInvitations;
            GameRemindersSwitch.IsToggled = _settings.GameReminders;
            FriendRequestsSwitch.IsToggled = _settings.FriendRequests;
            PostInteractionsSwitch.IsToggled = _settings.PostInteractions;
            SystemUpdatesSwitch.IsToggled = _settings.SystemUpdates;

            // Update time pickers
            if (TimeSpan.TryParse(_settings.QuietHoursStart, out TimeSpan startTime))
            {
                StartTimePicker.Time = startTime;
            }

            if (TimeSpan.TryParse(_settings.QuietHoursEnd, out TimeSpan endTime))
            {
                EndTimePicker.Time = endTime;
            }

            // Update quiet hours frame visibility
            UpdateQuietHoursFrameVisibility();
        }

        private void UpdateQuietHoursFrameVisibility()
        {
            // Show/hide quiet hours settings based on enabled state
            QuietHoursFrame.IsVisible = _settings.QuietHoursEnabled;
        }

        // Event handlers
        private void OnPushNotificationsToggled(object sender, ToggledEventArgs e)
        {
            _settings.EnablePushNotifications = e.Value;
        }

        private void OnEmailNotificationsToggled(object sender, ToggledEventArgs e)
        {
            _settings.EnableEmailNotifications = e.Value;
        }

        private void OnQuietHoursToggled(object sender, ToggledEventArgs e)
        {
            _settings.QuietHoursEnabled = e.Value;
            UpdateQuietHoursFrameVisibility();
        }

        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                // Update time settings from pickers
                _settings.QuietHoursStart = StartTimePicker.Time.ToString(@"hh\:mm");
                _settings.QuietHoursEnd = EndTimePicker.Time.ToString(@"hh\:mm");

                // In a real app, you would save settings to a service or storage
                // For example:
                // await _notificationService.UpdateNotificationSettingsAsync(_settings);

                await DisplayAlert("Success", "Notification settings have been saved", "OK");

                // Go back to previous page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving notification settings: {ex.Message}");
                await DisplayAlert("Error", "Could not save notification settings", "OK");
            }
        }

        private async void OnClearNotificationsClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Clear Notifications",
                "Are you sure you want to clear all notifications? This action cannot be undone.",
                "Yes", "No");

            if (confirm)
            {
                try
                {
                    // In a real app, you would call a service to clear notifications
                    // For example:
                    // await _notificationService.ClearAllNotificationsAsync();

                    await DisplayAlert("Success", "All notifications have been cleared", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing notifications: {ex.Message}");
                    await DisplayAlert("Error", "Could not clear notifications", "OK");
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}