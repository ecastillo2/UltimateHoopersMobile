using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UltimateHoopers.Pages
{
    public partial class AccountSettingsPage : ContentPage
    {
        public AccountSettingsPage()
        {
            InitializeComponent();
            InitializeUserData();
        }

        private void InitializeUserData()
        {
            // Populate labels with user data from your authentication service
            // This is just sample code - replace with your actual implementation
            UsernameLabel.Text = "johndoe";
            EmailLabel.Text = "john.doe@example.com";
            CurrentEmailLabel.Text = "john.doe@example.com";
            AccountTypeLabel.Text = "Free Player Account";
            JoinedDateLabel.Text = "January 15, 2023";
            CurrentPlanLabel.Text = "Free Plan";

            // Set version
            VersionLabel.Text = $"Version {AppInfo.Current.VersionString}";

            // Set subscription visibility based on user's subscription status
            bool isPremium = false; // Get from your subscription service
            UpgradeButton.IsVisible = !isPremium;
            CancelSubscriptionButton.IsVisible = isPremium;

            // Initialize plan features
            if (isPremium)
            {
                // Add premium features to the list
                AddFeatureToList("Advanced statistics");
                AddFeatureToList("Video analysis tools");
                AddFeatureToList("Unlimited posts");
                AddFeatureToList("Ad-free experience");
            }
            else
            {
                // Add free features to the list
                AddFeatureToList("Basic features included");
                AddFeatureToList("Limited statistics");
                AddFeatureToList("5 posts per week");
            }

            // Set two-factor auth status
            TwoFactorSwitch.IsToggled = false; // Get from your auth service
        }

        private void AddFeatureToList(string featureText)
        {
            var layout = new HorizontalStackLayout { Spacing = 10 };
            layout.Add(new Label { Text = "✓", TextColor = (Color)Application.Current.Resources["PrimaryColor"] });
            layout.Add(new Label { Text = featureText, TextColor = (Color)Application.Current.Resources["PrimaryTextColor"] });
            PlanFeaturesLayout.Add(layout);
        }

        #region Navigation
        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to home page
            Navigation.PopToRootAsync();
        }

        private void OnPostsNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to posts page
            // Implementation depends on your navigation structure
        }

        private void OnFAQNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to FAQ page
            // Implementation depends on your navigation structure
        }
        #endregion

        #region Email Management
        private void OnChangeEmailClicked(object sender, EventArgs e)
        {
            // Show the change email popup
            ChangeEmailPopup.IsVisible = true;
        }

        private void OnCancelEmailChangeClicked(object sender, EventArgs e)
        {
            // Hide the popup and clear fields
            ChangeEmailPopup.IsVisible = false;
            NewEmailEntry.Text = string.Empty;
            EmailPasswordEntry.Text = string.Empty;
        }

        private async void OnConfirmEmailChangeClicked(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NewEmailEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a new email address.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please enter your password to confirm this change.", "OK");
                return;
            }

            try
            {
                // In a real app, you would call your authentication service here
                // to update the email address

                // Simulate API call with delay
                await Task.Delay(1000);

                // Update UI
                EmailLabel.Text = NewEmailEntry.Text;
                CurrentEmailLabel.Text = NewEmailEntry.Text;

                // Hide popup and clear fields
                ChangeEmailPopup.IsVisible = false;
                NewEmailEntry.Text = string.Empty;
                EmailPasswordEntry.Text = string.Empty;

                await DisplayAlert("Success", "Your email has been updated successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update email: {ex.Message}", "OK");
            }
        }
        #endregion

        #region Password Management
        private void OnChangePasswordClicked(object sender, EventArgs e)
        {
            // Show the change password popup
            ClearPasswordFields();
            ChangePasswordPopup.IsVisible = true;
        }

        private void OnCancelPasswordChangeClicked(object sender, EventArgs e)
        {
            // Hide the popup and clear fields
            ChangePasswordPopup.IsVisible = false;
            ClearPasswordFields();
        }

        private void ClearPasswordFields()
        {
            CurrentPasswordEntry.Text = string.Empty;
            NewPasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;

            // Reset UI elements
            PasswordStrengthLabel.Text = "Password strength: Too weak";
            PasswordMatchLabel.IsVisible = false;
            UpdatePasswordButton.IsEnabled = false;

            // Reset strength indicators
            StrengthIndicator1.BackgroundColor = Color.FromArgb("#E0E0E0");
            StrengthIndicator2.BackgroundColor = Color.FromArgb("#E0E0E0");
            StrengthIndicator3.BackgroundColor = Color.FromArgb("#E0E0E0");
            StrengthIndicator4.BackgroundColor = Color.FromArgb("#E0E0E0");

            // Reset requirement icons
            LengthCheckIcon.Text = "○";
            LengthCheckIcon.TextColor = Color.FromArgb("#9E9E9E");

            UppercaseCheckIcon.Text = "○";
            UppercaseCheckIcon.TextColor = Color.FromArgb("#9E9E9E");

            LowercaseCheckIcon.Text = "○";
            LowercaseCheckIcon.TextColor = Color.FromArgb("#9E9E9E");

            NumberCheckIcon.Text = "○";
            NumberCheckIcon.TextColor = Color.FromArgb("#9E9E9E");

            SpecialCheckIcon.Text = "○";
            SpecialCheckIcon.TextColor = Color.FromArgb("#9E9E9E");
        }

        private async void OnConfirmPasswordChangeClicked(object sender, EventArgs e)
        {
            // Validate input should be done in real-time through the TextChanged events
            // But we can add extra validation here if needed

            try
            {
                // In a real app, you would call your authentication service here
                // to update the password

                // Simulate API call with delay
                await Task.Delay(1000);

                // Hide popup and clear fields
                ChangePasswordPopup.IsVisible = false;
                ClearPasswordFields();

                await DisplayAlert("Success", "Your password has been updated successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update password: {ex.Message}", "OK");
            }
        }

        // Password visibility toggle methods
        private void OnToggleCurrentPasswordVisibility(object sender, EventArgs e)
        {
            CurrentPasswordEntry.IsPassword = !CurrentPasswordEntry.IsPassword;
            ShowCurrentPasswordButton.Text = CurrentPasswordEntry.IsPassword ? "👁" : "👁‍🗨";
        }

        private void OnToggleNewPasswordVisibility(object sender, EventArgs e)
        {
            NewPasswordEntry.IsPassword = !NewPasswordEntry.IsPassword;
            ShowNewPasswordButton.Text = NewPasswordEntry.IsPassword ? "👁" : "👁‍🗨";
        }

        private void OnToggleConfirmPasswordVisibility(object sender, EventArgs e)
        {
            ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;
            ShowConfirmPasswordButton.Text = ConfirmPasswordEntry.IsPassword ? "👁" : "👁‍🗨";
        }

        // Password strength and validation methods
        private void OnNewPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            string password = NewPasswordEntry.Text ?? string.Empty;

            // Update requirement checkmarks
            bool hasLength = password.Length >= 8;
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasNumber = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            // Update requirement icons
            LengthCheckIcon.Text = hasLength ? "✓" : "○";
            LengthCheckIcon.TextColor = hasLength ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            UppercaseCheckIcon.Text = hasUppercase ? "✓" : "○";
            UppercaseCheckIcon.TextColor = hasUppercase ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            LowercaseCheckIcon.Text = hasLowercase ? "✓" : "○";
            LowercaseCheckIcon.TextColor = hasLowercase ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            NumberCheckIcon.Text = hasNumber ? "✓" : "○";
            NumberCheckIcon.TextColor = hasNumber ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            SpecialCheckIcon.Text = hasSpecial ? "✓" : "○";
            SpecialCheckIcon.TextColor = hasSpecial ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            // Calculate strength score (0-5)
            int strengthScore = 0;
            if (hasLength) strengthScore++;
            if (hasUppercase) strengthScore++;
            if (hasLowercase) strengthScore++;
            if (hasNumber) strengthScore++;
            if (hasSpecial) strengthScore++;

            // Update strength meter
            Color weakColor = Color.FromArgb("#F44336");    // Red
            Color fairColor = Color.FromArgb("#FF9800");    // Orange
            Color goodColor = Color.FromArgb("#2196F3");    // Blue
            Color strongColor = Color.FromArgb("#4CAF50");  // Green

            // Reset all indicators to default
            StrengthIndicator1.BackgroundColor = Color.FromArgb("#E0E0E0");
            StrengthIndicator2.BackgroundColor = Color.FromArgb("#E0E0E0");
            StrengthIndicator3.BackgroundColor = Color.FromArgb("#E0E0E0");
            StrengthIndicator4.BackgroundColor = Color.FromArgb("#E0E0E0");

            // Update strength text and indicators
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthLabel.Text = "Password strength: Too weak";
                PasswordStrengthLabel.TextColor = Color.FromArgb("#9E9E9E");
            }
            else if (strengthScore <= 2)
            {
                PasswordStrengthLabel.Text = "Password strength: Weak";
                PasswordStrengthLabel.TextColor = weakColor;
                StrengthIndicator1.BackgroundColor = weakColor;
            }
            else if (strengthScore == 3)
            {
                PasswordStrengthLabel.Text = "Password strength: Fair";
                PasswordStrengthLabel.TextColor = fairColor;
                StrengthIndicator1.BackgroundColor = fairColor;
                StrengthIndicator2.BackgroundColor = fairColor;
            }
            else if (strengthScore == 4)
            {
                PasswordStrengthLabel.Text = "Password strength: Good";
                PasswordStrengthLabel.TextColor = goodColor;
                StrengthIndicator1.BackgroundColor = goodColor;
                StrengthIndicator2.BackgroundColor = goodColor;
                StrengthIndicator3.BackgroundColor = goodColor;
            }
            else if (strengthScore == 5)
            {
                PasswordStrengthLabel.Text = "Password strength: Strong";
                PasswordStrengthLabel.TextColor = strongColor;
                StrengthIndicator1.BackgroundColor = strongColor;
                StrengthIndicator2.BackgroundColor = strongColor;
                StrengthIndicator3.BackgroundColor = strongColor;
                StrengthIndicator4.BackgroundColor = strongColor;
            }

            // Check for password match and update button state
            CheckPasswordsMatch();
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckPasswordsMatch();
        }

        private void CheckPasswordsMatch()
        {
            string newPassword = NewPasswordEntry.Text ?? string.Empty;
            string confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

            // Check if passwords match, but only show message when user has started typing confirmation
            if (!string.IsNullOrEmpty(confirmPassword))
            {
                PasswordMatchLabel.IsVisible = true;

                if (newPassword == confirmPassword)
                {
                    PasswordMatchLabel.Text = "✓ Passwords match";
                    PasswordMatchLabel.TextColor = Color.FromArgb("#4CAF50");
                }
                else
                {
                    PasswordMatchLabel.Text = "✗ Passwords don't match";
                    PasswordMatchLabel.TextColor = Color.FromArgb("#F44336");
                }
            }
            else
            {
                PasswordMatchLabel.IsVisible = false;
            }

            // Enable update button only when all conditions are met
            bool hasValidPassword = !string.IsNullOrEmpty(CurrentPasswordEntry.Text) &&
                                   !string.IsNullOrEmpty(NewPasswordEntry.Text) &&
                                   NewPasswordEntry.Text.Length >= 8 &&
                                   NewPasswordEntry.Text.Any(char.IsUpper) &&
                                   NewPasswordEntry.Text.Any(char.IsLower) &&
                                   NewPasswordEntry.Text.Any(char.IsDigit) &&
                                   NewPasswordEntry.Text.Any(c => !char.IsLetterOrDigit(c)) &&
                                   NewPasswordEntry.Text == ConfirmPasswordEntry.Text;

            UpdatePasswordButton.IsEnabled = hasValidPassword;
        }
        #endregion

        #region Subscription Management
        private async void OnUpgradeClicked(object sender, EventArgs e)
        {
            // Navigate to the subscription page or show subscription options
            bool confirmed = await DisplayAlert("Upgrade to Pro",
                "Would you like to upgrade to the Pro plan for $4.99/month?",
                "Purchase", "Cancel");

            if (confirmed)
            {
                // In a real app, you would integrate with your payment provider here
                // After successful payment, update the UI

                // For demo purposes:
                await DisplayAlert("Success", "Thank you for upgrading to Pro! Your account has been updated.", "OK");

                // Update UI to reflect new subscription
                CurrentPlanLabel.Text = "Pro Plan";
                UpgradeButton.IsVisible = false;
                CancelSubscriptionButton.IsVisible = true;

                // Clear existing features
                PlanFeaturesLayout.Clear();

                // Add label back
                PlanFeaturesLayout.Add(new Label
                {
                    Text = "Features:",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = (Color)Application.Current.Resources["PrimaryTextColor"]
                });

                // Add pro features
                AddFeatureToList("Advanced statistics");
                AddFeatureToList("Video analysis tools");
                AddFeatureToList("Unlimited posts");
                AddFeatureToList("Ad-free experience");
            }
        }

        private async void OnCancelSubscriptionClicked(object sender, EventArgs e)
        {
            bool confirmed = await DisplayAlert("Cancel Subscription",
                "Are you sure you want to cancel your Pro subscription? You will lose access to premium features at the end of your billing period.",
                "Yes, Cancel", "Keep Subscription");

            if (confirmed)
            {
                // In a real app, you would call your subscription service here

                await DisplayAlert("Subscription Cancelled",
                    "Your subscription has been cancelled. You will have access to Pro features until the end of your current billing period.",
                    "OK");

                // Update UI
                // In a real app, you might not change this immediately
                // but rather when the subscription actually expires
                CurrentPlanLabel.Text = "Free Plan (Pro until end of billing period)";
                UpgradeButton.IsVisible = true;
                CancelSubscriptionButton.IsVisible = false;
            }
        }
        #endregion

        #region Permissions & Settings
        private void OnTwoFactorToggled(object sender, ToggledEventArgs e)
        {
            // In a real app, you would call your authentication service here
            // to enable or disable two-factor authentication

            if (e.Value)
            {
                // Would typically navigate to a 2FA setup page in a real app
                DisplayAlert("Two-Factor Authentication",
                    "Two-factor authentication has been enabled for your account.",
                    "OK");
            }
            else
            {
                DisplayAlert("Two-Factor Authentication",
                    "Two-factor authentication has been disabled for your account.",
                    "OK");
            }
        }

        private void OnNotificationsToggled(object sender, ToggledEventArgs e)
        {
            // In a real app, you would update the user's notification preferences
            if (e.Value)
            {
                // Enable notifications logic
            }
            else
            {
                // Disable notifications logic
            }
        }

        private void OnLocationToggled(object sender, ToggledEventArgs e)
        {
            // In a real app, you would update the user's location permissions
            if (e.Value)
            {
                // Request location permissions if needed
            }
            else
            {
                // Handle disabling location
            }
        }
        #endregion
        private void OnToggleEmailPasswordVisibility(object sender, EventArgs e)
        {
            EmailPasswordEntry.IsPassword = !EmailPasswordEntry.IsPassword;
            ShowEmailPasswordButton.Text = EmailPasswordEntry.IsPassword ? "👁" : "👁‍🗨";
        }
        #region Account Deletion
        private async void OnDeleteAccountClicked(object sender, EventArgs e)
        {
            bool confirmed = await DisplayAlert("Delete Account",
                "Are you sure you want to delete your account? This action cannot be undone and all your data will be permanently lost.",
                "Delete Account", "Cancel");

            if (confirmed)
            {
                // Ask for password to confirm
                string password = await DisplayPromptAsync("Confirm Deletion",
                    "Please enter your password to confirm account deletion:",
                    "Delete", "Cancel",
                    placeholder: "Password",
                    maxLength: 50,
                    keyboard: Keyboard.Text);

                if (!string.IsNullOrEmpty(password))
                {
                    // In a real app, you would verify the password and then delete the account

                    // Simulate API call with delay
                    await Task.Delay(1000);

                    // Navigate back to login screen or app entry point
                    await DisplayAlert("Account Deleted",
                        "Your account has been successfully deleted. We're sorry to see you go!",
                        "OK");

                    // Navigate back to login
                    await Navigation.PopToRootAsync();

                    // In a real app, you would also sign the user out here
                }
            }
        }
        #endregion
    }
}