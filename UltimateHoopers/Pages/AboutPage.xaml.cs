using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace UltimateHoopers.Pages
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            InitializePageData();
        }

        private void InitializePageData()
        {
            // Set the version info
            VersionLabel.Text = $"Version {AppInfo.Current.VersionString}";

            // Set the app logo - in a real app, you would use your actual app logo resource
            // AppLogoImage.Source = ImageSource.FromResource("UltimateHoopers.Resources.Images.app_logo.png");
        }

        #region Navigation Methods
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
            // For example:
            // await Navigation.PushAsync(new PostsPage());
        }

        private void OnAccountNavigationClicked(object sender, TappedEventArgs e)
        {
            // Navigate to account settings page
            Navigation.PushAsync(new AccountSettingsPage());
        }
        #endregion

        #region Support and Legal Methods
        private async void OnContactSupportClicked(object sender, EventArgs e)
        {
            // Show options for contacting support
            string action = await DisplayActionSheet(
                "Contact Support",
                "Cancel",
                null,
                "Send Email",
                "Visit Website",
                "Call Support");

            switch (action)
            {
                case "Send Email":
                    // Open email client
                    await SendEmail("support@ultimatehoopers.com", "Support Request from Ultimate Hoopers App", "");
                    break;
                case "Visit Website":
                    // Open website
                    await OpenBrowser("https://www.ultimatehoopers.com/support");
                    break;
                case "Call Support":
                    // Make a phone call
                    await MakePhoneCall("+1-800-HOOPERS");
                    break;
            }
        }

        private async void OnTermsClicked(object sender, EventArgs e)
        {
            // Open terms of service
            await OpenBrowser("https://www.ultimatehoopers.com/terms");
        }

        private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
        {
            // Open privacy policy
            await OpenBrowser("https://www.ultimatehoopers.com/privacy");
        }

        private async void OnLicensesClicked(object sender, EventArgs e)
        {
            // Open licenses page
            await OpenBrowser("https://www.ultimatehoopers.com/licenses");
        }
        #endregion

        #region Helper Methods
        private async Task SendEmail(string to, string subject, string body)
        {
            try
            {
                var uri = new Uri($"mailto:{to}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}");
                await Launcher.OpenAsync(uri);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open email client: {ex.Message}", "OK");
            }
        }

        private async Task OpenBrowser(string url)
        {
            try
            {
                await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open browser: {ex.Message}", "OK");
            }
        }

        private async Task MakePhoneCall(string number)
        {
            try
            {
                var phoneDialer = PhoneDialer.Default;
                if (phoneDialer.IsSupported)
                {
                    phoneDialer.Open(number);
                }
                else
                {
                    await DisplayAlert("Not Supported", "Phone dialing is not supported on this device.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not place call: {ex.Message}", "OK");
            }
        }
        #endregion
    }
}