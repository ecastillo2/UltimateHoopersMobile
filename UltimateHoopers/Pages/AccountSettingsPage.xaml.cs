using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltimateHoopers.Helpers;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class AccountSettingsPage : ContentPage
    {
        private readonly IAuthService _authService;
        private bool _isPro = false;

        public AccountSettingsPage()
        {
            InitializeComponent();

            // Try to get auth service from DI
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();

            // Set app version
            VersionLabel.Text = $"Version {AppInfo.VersionString}";

            // Load user data when page appears
            this.Appearing += OnPageAppearing;
        }

        private async void OnPageAppearing(object sender, EventArgs e)
        {
            try
            {
                await LoadUserData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user data: {ex.Message}");
                await DisplayAlert("Error", "Could not load account information", "OK");
            }
        }

        private async Task LoadUserData()
        {
            // Check if user is logged in
            if (App.User == null)
            {
                await DisplayAlert("Not Logged In", "Please log in to view your account settings", "OK");
                await Navigation.PopAsync();
                return;
            }

            // Set user information
            UsernameLabel.Text = App.User.UserName ?? "username";
            EmailLabel.Text = App.User.Email ?? "email@example.com";
            CurrentEmailLabel.Text = App.User.Email ?? "email@example.com";

            // Set account type
            string accountType = "Free Player Account";
            if (App.User.AccountType == Domain.AccountType.Host)
            {
                accountType = "Court Host Account";
            }
            AccountTypeLabel.Text = accountType;

            // Subscription status
            // This is where you would check the user's actual subscription status
            // For this example, we'll use a simple approach
            CheckSubscriptionStatus();

            // Set joined date
            // In a real app, you would get this from the user object
            JoinedDateLabel.Text = "January 1, 2023";

            // Set permissions based on user preferences
            // In a real app, you would fetch these from user settings
            NotificationsSwitch.IsToggled = true;
            LocationSwitch.IsToggled = true;
            TwoFactorSwitch.IsToggled = false;
        }

        private void CheckSubscriptionStatus()
        {
            // This is a placeholder for real subscription logic
            // In a real app, you would check the user's subscription from a service

            // For demo purposes, we'll determine if the user has a Pro account
            // based on a simple condition - you would replace this with actual logic
            _isPro = App.User.UserName?.Contains("pro", StringComparison.OrdinalIgnoreCase) == true;

            if (_isPro)
            {
                // Update UI for Pro users
                CurrentPlanLabel.Text = "Pro Plan";
                UpgradeButton.Text = "Manage Subscription";
                CancelSubscriptionButton.IsVisible = true;

                // Update features list for Pro users
                PlanFeaturesLayout.Children.Clear();

                // Add header
                PlanFeaturesLayout.Children.Add(new Label
                {
                    Text = "Features",
                    FontSize = 14,
                    TextColor = (Color)Application.Current.Resources["SecondaryTextColor"]
                });

                // Add Pro features
                AddFeatureItem("Join and host unlimited games");
                AddFeatureItem("Advanced statistics and analytics");
                AddFeatureItem("Priority court bookings");
                AddFeatureItem("Ad-free experience");
                AddFeatureItem("Premium support");
            }
            else
            {
                // Free user UI is already set up in XAML
                CancelSubscriptionButton.IsVisible = false;
            }
        }

        private void AddFeatureItem(string featureText)
        {
            var stack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            stack.Children.Add(new Label
            {
                Text = "✓",
                TextColor = (Color)Application.Current.Resources["PrimaryColor"],
                FontSize = 16,
                VerticalOptions = LayoutOptions.Start
            });

            stack.Children.Add(new Label
            {
                Text = featureText,
                TextColor = (Color)Application.Current.Resources["PrimaryTextColor"],
                FontSize = 14
            });

            PlanFeaturesLayout.Children.Add(stack);
        }

        #region Navigation Handlers
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("AccountSettingsPage: OnHomeClicked - using DirectNavigationHelper");
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AccountSettingsPage: Error navigating to HomePage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnPostsNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("AccountSettingsPage: OnPostsClicked - using DirectNavigationHelper");
                await DirectNavigationHelper.GoToPostsPageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AccountSettingsPage: Error navigating to PostsPage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to posts page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnFAQNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new FAQPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AccountSettingsPage: Error navigating to FAQPage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to FAQ page. Please try again or restart the app.",
                    "OK");
            }
        }
        #endregion

        #region Account Settings Handlers
        private void OnChangeEmailClicked(object sender, EventArgs e)
        {
            // Show change email popup
            ChangeEmailPopup.IsVisible = true;
        }

        private void OnCancelEmailChangeClicked(object sender, EventArgs e)
        {
            // Hide popup and clear fields
            ChangeEmailPopup.IsVisible = false;
            NewEmailEntry.Text = string.Empty;
            EmailPasswordEntry.Text = string.Empty;
        }

        private async void OnConfirmEmailChangeClicked(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(NewEmailEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a new email address", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmailPasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your password", "OK");
                    return;
                }

                if (!IsValidEmail(NewEmailEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a valid email address", "OK");
                    return;
                }

                // Verify password (in a real app, you would check this with your auth service)
                bool passwordValid = true; // Replace with actual validation

                if (!passwordValid)
                {
                    await DisplayAlert("Error", "Incorrect password", "OK");
                    return;
                }

                // Update email (in a real app, you would call your auth service)
                string newEmail = NewEmailEntry.Text;

                // Show loading indicator
                await DisplayAlert("Email Update", "Processing your request...", "OK");

                // Simulate API call
                await Task.Delay(1000);

                // Update UI
                EmailLabel.Text = newEmail;
                CurrentEmailLabel.Text = newEmail;

                // Update user object
                if (App.User != null)
                {
                    App.User.Email = newEmail;
                }

                // Hide popup and clear fields
                ChangeEmailPopup.IsVisible = false;
                NewEmailEntry.Text = string.Empty;
                EmailPasswordEntry.Text = string.Empty;

                // Show success message
                await DisplayAlert("Success", "Your email has been updated successfully", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error changing email: {ex.Message}");
                await DisplayAlert("Error", "Failed to update email. Please try again later.", "OK");
            }
        }

        private void OnChangePasswordClicked(object sender, EventArgs e)
        {
            // Show change password popup
            ChangePasswordPopup.IsVisible = true;
        }

        private void OnCancelPasswordChangeClicked(object sender, EventArgs e)
        {
            // Hide popup and clear fields
            ChangePasswordPopup.IsVisible = false;
            CurrentPasswordEntry.Text = string.Empty;
            NewPasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;
        }

        private async void OnConfirmPasswordChangeClicked(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(CurrentPasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your current password", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a new password", "OK");
                    return;
                }

                if (NewPasswordEntry.Text.Length < 8)
                {
                    await DisplayAlert("Error", "Password must be at least 8 characters long", "OK");
                    return;
                }

                if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
                {
                    await DisplayAlert("Error", "New passwords do not match", "OK");
                    return;
                }

                // Verify current password (in a real app, you would check this with your auth service)
                bool passwordValid = true; // Replace with actual validation

                if (!passwordValid)
                {
                    await DisplayAlert("Error", "Incorrect current password", "OK");
                    return;
                }

                // Update password (in a real app, you would call your auth service)
                string newPassword = NewPasswordEntry.Text;

                // Show loading indicator
                await DisplayAlert("Password Update", "Processing your request...", "OK");

                // Simulate API call
                await Task.Delay(1000);

                // Hide popup and clear fields
                ChangePasswordPopup.IsVisible = false;
                CurrentPasswordEntry.Text = string.Empty;
                NewPasswordEntry.Text = string.Empty;
                ConfirmPasswordEntry.Text = string.Empty;

                // Show success message
                await DisplayAlert("Success", "Your password has been updated successfully", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error changing password: {ex.Message}");
                await DisplayAlert("Error", "Failed to update password. Please try again later.", "OK");
            }
        }

        private async void OnTwoFactorToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                bool isEnabled = e.Value;

                if (isEnabled)
                {
                    // In a real app, you would start the 2FA setup process
                    bool confirm = await DisplayAlert("Enable 2FA",
                        "Two-factor authentication adds an extra layer of security to your account. Would you like to set it up now?",
                        "Yes", "No");

                    if (confirm)
                    {
                        // This would navigate to a 2FA setup page in a real app
                        await DisplayAlert("2FA Setup",
                            "This would start the 2FA setup process in a real application. For now, we'll simulate that it was enabled successfully.",
                            "OK");
                    }
                    else
                    {
                        // User canceled, revert the switch
                        TwoFactorSwitch.IsToggled = false;
                    }
                }
                else
                {
                    // Confirm disabling 2FA
                    bool confirm = await DisplayAlert("Disable 2FA",
                        "Are you sure you want to disable two-factor authentication? This will reduce the security of your account.",
                        "Disable", "Cancel");

                    if (!confirm)
                    {
                        // User canceled, revert the switch
                        TwoFactorSwitch.IsToggled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling 2FA: {ex.Message}");
                await DisplayAlert("Error", "Failed to update two-factor authentication settings", "OK");

                // Reset the switch to its previous state
                TwoFactorSwitch.IsToggled = !e.Value;
            }
        }

        private async void OnNotificationsToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                bool isEnabled = e.Value;

                // In a real app, you would update user preferences
                await Task.Delay(100); // Simulate saving preference

                string status = isEnabled ? "enabled" : "disabled";
                await DisplayAlert("Notifications", $"Push notifications have been {status}", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling notifications: {ex.Message}");
                await DisplayAlert("Error", "Failed to update notification settings", "OK");

                // Reset the switch to its previous state
                NotificationsSwitch.IsToggled = !e.Value;
            }
        }

        private async void OnLocationToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                //bool isEnabled = e.Value;

                //if (isEnabled)
                //{
                //    // Request location permission
                //    //var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                //    var status = true;

                //    if (status != PermissionStatus.Granted)
                //    {
                //        // Permission denied, revert the switch
                //        LocationSwitch.IsToggled = false;

                //        await DisplayAlert("Permission Required",
                //            "Location permission is required for this feature. Please enable it in your device settings.",
                //            "OK");

                //        return;
                //    }
                //}

                // In a real app, you would update user preferences
                await Task.Delay(100); // Simulate saving preference

                //string status = isEnabled ? "enabled" : "disabled";
                await DisplayAlert("Location Services", $"Location services have been ", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling location: {ex.Message}");
                await DisplayAlert("Error", "Failed to update location settings", "OK");

                // Reset the switch to its previous state
                LocationSwitch.IsToggled = !e.Value;
            }
        }

        private async void OnDeleteAccountClicked(object sender, EventArgs e)
        {
            try
            {
                // Confirm account deletion
                bool confirm = await DisplayAlert("Delete Account",
                    "Are you sure you want to delete your account? This action cannot be undone and all your data will be permanently lost.",
                    "Delete", "Cancel");

                if (!confirm)
                {
                    return;
                }

                // Double confirm
                bool confirmAgain = await DisplayAlert("Final Confirmation",
                    "Please confirm once more that you want to permanently delete your account.",
                    "Delete Account", "Cancel");

                if (!confirmAgain)
                {
                    return;
                }

                // Show loading indicator
                await DisplayAlert("Account Deletion", "Processing your request...", "OK");

                // In a real app, you would call your auth service
                if (_authService != null)
                {
                    await _authService.LogoutAsync();
                }
                else
                {
                    // Manual logout
                    App.AuthToken = null;
                    App.User = null;
                    await SecureStorage.Default.SetAsync("auth_token", string.Empty);
                    await SecureStorage.Default.SetAsync("user_id", string.Empty);
                }

                // Simulate API call
                await Task.Delay(1000);

                // Navigate to login page
                Application.Current.MainPage = new LoginPage();

                // Show success message
                await Application.Current.MainPage.DisplayAlert("Account Deleted",
                    "Your account has been successfully deleted. We're sorry to see you go!",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting account: {ex.Message}");
                await DisplayAlert("Error", "Failed to delete account. Please try again later.", "OK");
            }
        }

        private async void OnUpgradeClicked(object sender, EventArgs e)
        {
            try
            {
                if (_isPro)
                {
                    // For Pro users, this shows subscription management
                    await DisplayAlert("Manage Subscription",
                        "This would open the subscription management page in a real application.",
                        "OK");
                }
                else
                {
                    // For free users, show upgrade options
                    string action = await DisplayActionSheet("Upgrade to Pro",
                        "Cancel",
                        null,
                        "Monthly Plan - $4.99/month",
                        "Annual Plan - $49.99/year (Save 16%)",
                        "Lifetime Access - $199.99");

                    if (action != "Cancel" && !string.IsNullOrEmpty(action))
                    {
                        // In a real app, this would navigate to payment page
                        await Navigation.PushAsync(new PaymentPage());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling upgrade: {ex.Message}");
                await DisplayAlert("Error", "Failed to process upgrade request. Please try again later.", "OK");
            }
        }

        private async void OnCancelSubscriptionClicked(object sender, EventArgs e)
        {
            try
            {
                // Confirm subscription cancellation
                bool confirm = await DisplayAlert("Cancel Subscription",
                    "Are you sure you want to cancel your Pro subscription? You will lose access to Pro features at the end of your current billing period.",
                    "Cancel Subscription", "Keep Subscription");

                if (!confirm)
                {
                    return;
                }

                // Show loading indicator
                await DisplayAlert("Subscription Cancellation", "Processing your request...", "OK");

                // Simulate API call
                await Task.Delay(1000);

                // Update UI to reflect changes
                _isPro = false;
                CurrentPlanLabel.Text = "Free Plan (Pro until end of billing period)";
                CancelSubscriptionButton.IsVisible = false;
                UpgradeButton.Text = "Resubscribe";

                // Show success message
                await DisplayAlert("Subscription Cancelled",
                    "Your subscription has been cancelled. You will continue to have access to Pro features until the end of your current billing period.",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cancelling subscription: {ex.Message}");
                await DisplayAlert("Error", "Failed to cancel subscription. Please try again later.", "OK");
            }
        }
        #endregion

        #region Helper Methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}