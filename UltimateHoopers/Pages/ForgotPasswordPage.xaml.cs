using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class ForgotPasswordPage : ContentPage
    {
        private readonly IAuthService _authService;
        private string _verificationCode;
        private string _userEmail;
        private bool _isResendAvailable = true;
        private int _resendCountdown = 60;

        // Current step tracking
        private enum ResetStep
        {
            Email = 1,
            Verification = 2,
            NewPassword = 3,
            Success = 4
        }

        private ResetStep _currentStep = ResetStep.Email;

        // Property to store the previous page for navigation back
        public Page PreviousPage { get; set; }

        // Default constructor
        public ForgotPasswordPage()
        {
            InitializeComponent();

            // Try to get auth service from DI
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _authService = serviceProvider.GetService<IAuthService>();
        }

        // Constructor with auth service
        public ForgotPasswordPage(IAuthService authService) : this()
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        #region Navigation Events

        private async void OnBackClicked(object sender, EventArgs e)
        {
            if (_currentStep == ResetStep.Email)
            {
                // Go back to login page
                await NavigateToLoginPage();
            }
            else
            {
                // Go back to previous step
                await GoToPreviousStep();
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await NavigateToLoginPage();
        }

        private async Task NavigateToLoginPage()
        {
            try
            {
                // Try navigation methods in order of preference
                if (Navigation != null && Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
                else if (PreviousPage != null)
                {
                    Application.Current.MainPage = PreviousPage;
                }
                else
                {
                    // Create new login page
                    LoginPage loginPage;
                    if (_authService != null)
                    {
                        loginPage = new LoginPage(_authService);
                    }
                    else
                    {
                        loginPage = new LoginPage();
                    }
                    Application.Current.MainPage = loginPage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to login: {ex.Message}");
                // Fallback
                Application.Current.MainPage = new LoginPage();
            }
        }

        #endregion

        #region Step 1: Email Input

        private async void OnSendCodeClicked(object sender, EventArgs e)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(EmailEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter your email address", "OK");
                    return;
                }

                if (!IsValidEmail(EmailEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter a valid email address", "OK");
                    return;
                }

                // Update UI
                SendCodeButton.IsEnabled = false;
                SendCodeButton.Text = "Sending...";

                _userEmail = EmailEntry.Text.Trim();

                // Send verification code
                bool success = await SendVerificationCode(_userEmail);

                if (success)
                {
                    // Move to verification step
                    await GoToStep(ResetStep.Verification);
                }
                else
                {
                    await DisplayAlert("Error", "Failed to send verification code. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending code: {ex.Message}");
                await DisplayAlert("Error", "An error occurred. Please try again.", "OK");
            }
            finally
            {
                SendCodeButton.IsEnabled = true;
                SendCodeButton.Text = "Send Verification Code";
            }
        }

        private async Task<bool> SendVerificationCode(string email)
        {
            try
            {
                // Generate a random 6-digit code
                var random = new Random();
                _verificationCode = random.Next(100000, 999999).ToString();

                System.Diagnostics.Debug.WriteLine($"Generated verification code: {_verificationCode} for email: {email}");

                // In a real app, you would call your auth service here
                if (_authService != null)
                {
                    // Example: await _authService.SendPasswordResetCodeAsync(email);
                    await Task.Delay(1000); // Simulate API call
                    return true;
                }
                else
                {
                    // Simulate sending email
                    await Task.Delay(1000);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendVerificationCode: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Step 2: Code Verification

        private void OnCodeEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry == null) return;

            // Auto-advance to next field
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length == 1)
            {
                AdvanceToNextCodeEntry(entry);
            }

            // Check if all code fields are filled
            ValidateCodeEntries();
        }

        private void AdvanceToNextCodeEntry(Entry currentEntry)
        {
            // Find the next entry to focus
            if (currentEntry == Code1Entry) Code2Entry.Focus();
            else if (currentEntry == Code2Entry) Code3Entry.Focus();
            else if (currentEntry == Code3Entry) Code4Entry.Focus();
            else if (currentEntry == Code4Entry) Code5Entry.Focus();
            else if (currentEntry == Code5Entry) Code6Entry.Focus();
        }

        private void ValidateCodeEntries()
        {
            // Check if all code entries have text
            bool allFilled = !string.IsNullOrEmpty(Code1Entry.Text) &&
                            !string.IsNullOrEmpty(Code2Entry.Text) &&
                            !string.IsNullOrEmpty(Code3Entry.Text) &&
                            !string.IsNullOrEmpty(Code4Entry.Text) &&
                            !string.IsNullOrEmpty(Code5Entry.Text) &&
                            !string.IsNullOrEmpty(Code6Entry.Text);

            VerifyCodeButton.IsEnabled = allFilled;
        }

        private async void OnVerifyCodeClicked(object sender, EventArgs e)
        {
            try
            {
                // Get entered code
                string enteredCode = Code1Entry.Text + Code2Entry.Text + Code3Entry.Text +
                                   Code4Entry.Text + Code5Entry.Text + Code6Entry.Text;

                // Update UI
                VerifyCodeButton.IsEnabled = false;
                VerifyCodeButton.Text = "Verifying...";

                // Verify code
                bool isValid = await VerifyCode(enteredCode);

                if (isValid)
                {
                    // Move to password reset step
                    await GoToStep(ResetStep.NewPassword);
                }
                else
                {
                    await DisplayAlert("Invalid Code", "The verification code you entered is incorrect. Please try again.", "OK");
                    ClearCodeEntries();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verifying code: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while verifying the code. Please try again.", "OK");
            }
            finally
            {
                VerifyCodeButton.IsEnabled = true;
                VerifyCodeButton.Text = "Verify Code";
            }
        }

        private async Task<bool> VerifyCode(string enteredCode)
        {
            try
            {
                // In a real app, you would verify with your auth service
                if (_authService != null)
                {
                    // Example: return await _authService.VerifyPasswordResetCodeAsync(_userEmail, enteredCode);
                    await Task.Delay(1000); // Simulate API call
                    return enteredCode == _verificationCode;
                }
                else
                {
                    // Simulate verification
                    await Task.Delay(1000);
                    return enteredCode == _verificationCode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in VerifyCode: {ex.Message}");
                return false;
            }
        }

        private void ClearCodeEntries()
        {
            Code1Entry.Text = string.Empty;
            Code2Entry.Text = string.Empty;
            Code3Entry.Text = string.Empty;
            Code4Entry.Text = string.Empty;
            Code5Entry.Text = string.Empty;
            Code6Entry.Text = string.Empty;
            Code1Entry.Focus();
        }

        private async void OnResendCodeClicked(object sender, EventArgs e)
        {
            if (!_isResendAvailable) return;

            try
            {
                bool success = await SendVerificationCode(_userEmail);

                if (success)
                {
                    await DisplayAlert("Code Sent", "A new verification code has been sent to your email.", "OK");
                    ClearCodeEntries();
                    StartResendCountdown();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to resend verification code. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resending code: {ex.Message}");
                await DisplayAlert("Error", "An error occurred. Please try again.", "OK");
            }
        }

        private async void StartResendCountdown()
        {
            _isResendAvailable = false;
            _resendCountdown = 60;
            ResendCodeLabel.IsVisible = false;
            CountdownLabel.IsVisible = true;

            while (_resendCountdown > 0)
            {
                CountdownLabel.Text = $"Resend available in {_resendCountdown} seconds";
                await Task.Delay(1000);
                _resendCountdown--;
            }

            _isResendAvailable = true;
            CountdownLabel.IsVisible = false;
            ResendCodeLabel.IsVisible = true;
        }

        #endregion

        #region Step 3: New Password

        private void OnNewPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePassword(e.NewTextValue);
            CheckPasswordsMatch();
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckPasswordsMatch();
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ResetPasswordRequirements();
                return;
            }

            // Check requirements
            bool hasLength = password.Length >= 8;
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasNumber = password.Any(char.IsDigit);

            // Update requirement icons
            LengthCheckIcon.Text = hasLength ? "✓" : "○";
            LengthCheckIcon.TextColor = hasLength ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            UppercaseCheckIcon.Text = hasUppercase ? "✓" : "○";
            UppercaseCheckIcon.TextColor = hasUppercase ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            LowercaseCheckIcon.Text = hasLowercase ? "✓" : "○";
            LowercaseCheckIcon.TextColor = hasLowercase ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

            NumberCheckIcon.Text = hasNumber ? "✓" : "○";
            NumberCheckIcon.TextColor = hasNumber ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");
        }

        private void ResetPasswordRequirements()
        {
            LengthCheckIcon.Text = "○";
            LengthCheckIcon.TextColor = Color.FromArgb("#9E9E9E");
            UppercaseCheckIcon.Text = "○";
            UppercaseCheckIcon.TextColor = Color.FromArgb("#9E9E9E");
            LowercaseCheckIcon.Text = "○";
            LowercaseCheckIcon.TextColor = Color.FromArgb("#9E9E9E");
            NumberCheckIcon.Text = "○";
            NumberCheckIcon.TextColor = Color.FromArgb("#9E9E9E");
        }

        private void CheckPasswordsMatch()
        {
            string newPassword = NewPasswordEntry.Text ?? string.Empty;
            string confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

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

            // Enable reset button only when all conditions are met
            bool hasValidPassword = !string.IsNullOrEmpty(newPassword) &&
                                   newPassword.Length >= 8 &&
                                   newPassword.Any(char.IsUpper) &&
                                   newPassword.Any(char.IsLower) &&
                                   newPassword.Any(char.IsDigit) &&
                                   newPassword == confirmPassword;

            ResetPasswordButton.IsEnabled = hasValidPassword;
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

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            try
            {
                // Update UI
                ResetPasswordButton.IsEnabled = false;
                ResetPasswordButton.Text = "Resetting...";

                // Reset password
                bool success = await ResetPassword(NewPasswordEntry.Text);

                if (success)
                {
                    // Move to success step
                    await GoToStep(ResetStep.Success);
                }
                else
                {
                    await DisplayAlert("Error", "Failed to reset password. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting password: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while resetting your password. Please try again.", "OK");
            }
            finally
            {
                ResetPasswordButton.IsEnabled = true;
                ResetPasswordButton.Text = "Reset Password";
            }
        }

        private async Task<bool> ResetPassword(string newPassword)
        {
            try
            {
                // In a real app, you would call your auth service here
                if (_authService != null)
                {
                    // Example: return await _authService.ResetPasswordAsync(_userEmail, _verificationCode, newPassword);
                    await Task.Delay(1000); // Simulate API call
                    return true;
                }
                else
                {
                    // Simulate password reset
                    await Task.Delay(1000);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ResetPassword: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Step Management

        private async Task GoToStep(ResetStep step)
        {
            _currentStep = step;

            // Hide all steps
            EmailStep.IsVisible = false;
            CodeStep.IsVisible = false;
            PasswordStep.IsVisible = false;
            SuccessStep.IsVisible = false;

            // Update step indicators
            UpdateStepIndicators(step);

            // Show current step
            switch (step)
            {
                case ResetStep.Email:
                    EmailStep.IsVisible = true;
                    break;
                case ResetStep.Verification:
                    CodeStep.IsVisible = true;
                    SentToEmailLabel.Text = $"Code sent to: {_userEmail}";
                    Code1Entry.Focus();
                    StartResendCountdown();
                    break;
                case ResetStep.NewPassword:
                    PasswordStep.IsVisible = true;
                    NewPasswordEntry.Focus();
                    break;
                case ResetStep.Success:
                    SuccessStep.IsVisible = true;
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task GoToPreviousStep()
        {
            switch (_currentStep)
            {
                case ResetStep.Verification:
                    await GoToStep(ResetStep.Email);
                    break;
                case ResetStep.NewPassword:
                    await GoToStep(ResetStep.Verification);
                    break;
                default:
                    await NavigateToLoginPage();
                    break;
            }
        }

        private void UpdateStepIndicators(ResetStep currentStep)
        {
            // Reset all indicators
            ResetStepIndicator(Step1Circle, 1);
            ResetStepIndicator(Step2Circle, 2);
            ResetStepIndicator(Step3Circle, 3);

            // Update current and completed steps
            for (int i = 1; i <= (int)currentStep; i++)
            {
                Frame circle = i switch
                {
                    1 => Step1Circle,
                    2 => Step2Circle,
                    3 => Step3Circle,
                    _ => null
                };

                if (circle != null)
                {
                    SetStepIndicatorActive(circle, i);
                }
            }
        }

        private void ResetStepIndicator(Frame circle, int stepNumber)
        {
            circle.BackgroundColor = (Color)Application.Current.Resources["BorderColor"];
            circle.BorderColor = (Color)Application.Current.Resources["BorderColor"];

            if (circle.Content is Label label)
            {
                label.TextColor = (Color)Application.Current.Resources["SecondaryTextColor"];
            }
        }

        private void SetStepIndicatorActive(Frame circle, int stepNumber)
        {
            circle.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
            circle.BorderColor = (Color)Application.Current.Resources["PrimaryColor"];

            if (circle.Content is Label label)
            {
                label.TextColor = Colors.White;
            }
        }

        #endregion

        #region Utility Methods

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}