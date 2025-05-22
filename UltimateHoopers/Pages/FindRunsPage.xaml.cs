using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UltimateHoopers.Converter;
using UltimateHoopers.Helpers;
using UltimateHoopers.Models;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using System.Collections.Generic;
using UltimateHoopers.ViewModels.UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class FindRunsPage : ContentPage
    {
        private PrivateRunsViewModel _viewModel;
        private IAuthService _authService;
        private IPrivateRunService _privateRunService;
        private bool _isUserHost = false;

        public FindRunsPage()
        {
            InitializeComponent();
            InitializeServices();
            InitializeViewModel();
            SetupUI();

            Debug.WriteLine("=== FindRunsPage Constructor Complete ===");
        }

        public FindRunsPage(IAuthService authService = null, IPrivateRunService privateRunService = null)
        {
            InitializeComponent();
            _authService = authService;
            _privateRunService = privateRunService;
            InitializeViewModel();
            SetupUI();

            Debug.WriteLine("=== FindRunsPage Constructor with DI Complete ===");
        }

        private void InitializeServices()
        {
            try
            {
                // Try to get services from DI container
                var serviceProvider = MauiProgram.CreateMauiApp()?.Services;

                if (serviceProvider != null)
                {
                    _authService ??= serviceProvider.GetService<IAuthService>();
                    _privateRunService ??= serviceProvider.GetService<IPrivateRunService>();
                }

                Debug.WriteLine($"Services initialized - AuthService: {_authService != null}, PrivateRunService: {_privateRunService != null}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing services: {ex.Message}");
                // Services will be null, but app can still function with fallbacks
            }
        }

        private void InitializeViewModel()
        {
            try
            {
                _viewModel = new PrivateRunsViewModel(_privateRunService);
                BindingContext = _viewModel;

                Debug.WriteLine($"ViewModel created and bound. BindingContext is null: {BindingContext == null}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ViewModel: {ex.Message}");
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("=== FindRunsPage OnAppearing Start ===");

            try
            {
                // Check host status and update Create Run button
                await CheckHostStatusAndUpdateButton();

                // Load data when page appears
                if (_viewModel?.LoadRunsCommand?.CanExecute(null) == true)
                {
                    _viewModel.LoadRunsCommand.Execute(null);
                }

                Debug.WriteLine($"After LoadRunsCommand - Runs count: {_viewModel?.Runs?.Count ?? -1}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "Could not load run data. Please try again later.", "OK");
            }

            Debug.WriteLine("=== FindRunsPage OnAppearing End ===");
        }

        private async Task CheckHostStatusAndUpdateButton()
        {
            try
            {
                Debug.WriteLine("=== CheckHostStatusAndUpdateButton Start ===");

                _isUserHost = await DetermineHostStatus();
                Debug.WriteLine($"Host status determined: {_isUserHost}");

                UpdateCreateRunButton();
                Debug.WriteLine("=== CheckHostStatusAndUpdateButton End ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking host status: {ex.Message}");
                _isUserHost = false;
                UpdateCreateRunButton();
            }
        }

        private async Task<bool> DetermineHostStatus()
        {
            try
            {
                // Method 1: Check using AuthService (preferred)
                if (_authService != null)
                {
                    Debug.WriteLine("Checking host status via AuthService...");
                    bool isHost = await _authService.IsUserHostAsync();
                    Debug.WriteLine($"AuthService result: {isHost}");
                    if (isHost) return true;
                }

                // Method 2: Check App.User directly (fallback)
                if (App.User != null)
                {
                    Debug.WriteLine($"Checking App.User.AccountType: {App.User.AccountType}");
                    Debug.WriteLine($"Checking App.User.IsHost: {App.User.IsHost}");

                    bool isHost = App.User.AccountType == Domain.AccountType.Host || (App.User.IsHost ?? false);
                    Debug.WriteLine($"App.User result: {isHost}");
                    if (isHost) return true;
                }

                // Method 3: Check secure storage (last resort)
                try
                {
                    Debug.WriteLine("Checking secure storage for user data...");
                    string userData = await SecureStorage.GetAsync("user_data");

                    if (!string.IsNullOrEmpty(userData))
                    {
                        var user = System.Text.Json.JsonSerializer.Deserialize<Domain.User>(userData,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                        if (user != null)
                        {
                            bool isHost = user.AccountType == Domain.AccountType.Host || (user.IsHost ?? false);
                            Debug.WriteLine($"Secure storage result: {isHost}");
                            return isHost;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking secure storage: {ex.Message}");
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DetermineHostStatus: {ex.Message}");
                return false;
            }
        }

        private void UpdateCreateRunButton()
        {
            try
            {
                Debug.WriteLine($"=== UpdateCreateRunButton: IsHost = {_isUserHost} ===");

                if (CreateRunButton == null)
                {
                    Debug.WriteLine("WARNING: CreateRunButton not found!");
                    return;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (_isUserHost)
                        {
                            // User is a host - show create run button
                            CreateRunButton.Text = "⭐ Create Run";
                            CreateRunButton.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
                            CreateRunButton.TextColor = Colors.White;
                            CreateRunButton.BorderColor = Colors.Transparent;
                            CreateRunButton.BorderWidth = 0;
                            Debug.WriteLine("Button updated for HOST user");
                        }
                        else
                        {
                            // User is not a host - show upgrade button
                            CreateRunButton.Text = "⬆️ Upgrade to Host";
                            CreateRunButton.BackgroundColor = Colors.Orange;
                            CreateRunButton.TextColor = Colors.White;
                            CreateRunButton.BorderColor = Colors.DarkOrange;
                            CreateRunButton.BorderWidth = 1;
                            Debug.WriteLine("Button updated for FREE user");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating button on main thread: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating create run button: {ex.Message}");
            }
        }

        private async void OnCreateRunButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine($"=== OnCreateRunButtonClicked: IsHost = {_isUserHost} ===");

                if (_isUserHost)
                {
                    Debug.WriteLine("Navigating to CreateRunPage...");
                    await Navigation.PushAsync(new CreateRunPage());
                }
                else
                {
                    Debug.WriteLine("Showing upgrade options...");
                    await ShowUpgradeToHostDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling create run button click: {ex.Message}");
                await DisplayAlert("Error", "Unable to process request. Please try again.", "OK");
            }
        }

        private async Task ShowUpgradeToHostDialog()
        {
            try
            {
                bool upgrade = await DisplayAlert(
                    "Host Account Required",
                    "Creating runs requires a Host account ($9.99/month).\n\n" +
                    "Host Benefits:\n" +
                    "• Create unlimited runs\n" +
                    "• Manage player lists\n" +
                    "• Set custom pricing\n" +
                    "• Priority support\n" +
                    "• Advanced analytics\n\n" +
                    "Would you like to upgrade your account?",
                    "Upgrade Now",
                    "Maybe Later");

                if (upgrade)
                {
                    await HandleUpgradeFlow();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing upgrade dialog: {ex.Message}");
            }
        }

        private async Task HandleUpgradeFlow()
        {
            try
            {
                string action = await DisplayActionSheet(
                    "Choose Upgrade Method",
                    "Cancel",
                    null,
                    "💳 In-App Purchase ($9.99)",
                    "🎫 Enter Upgrade Code",
                    "📞 Contact Support");

                switch (action)
                {
                    case "💳 In-App Purchase ($9.99)":
                        await ProcessInAppPurchase();
                        break;
                    case "🎫 Enter Upgrade Code":
                        await ProcessUpgradeCode();
                        break;
                    case "📞 Contact Support":
                        await ShowContactSupport();
                        break;
                    default:
                        Debug.WriteLine("User cancelled upgrade");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling upgrade flow: {ex.Message}");
                await DisplayAlert("Error", "Unable to process upgrade. Please try again.", "OK");
            }
        }

        private async Task ProcessInAppPurchase()
        {
            try
            {
                Debug.WriteLine("Processing in-app purchase...");

                // Show processing indicator
                var isProcessing = true;
                var loadingTask = Task.Run(async () =>
                {
                    while (isProcessing)
                    {
                        await Task.Delay(100);
                    }
                });

                // Simulate payment processing (replace with real payment SDK)
                await Task.Delay(2000);
                bool paymentSuccess = true; // Replace with actual payment result

                isProcessing = false;
                await loadingTask;

                if (paymentSuccess)
                {
                    bool upgradeSuccess = false;

                    if (_authService != null)
                    {
                        upgradeSuccess = await _authService.UpgradeToHostAccountAsync();
                    }
                    else if (App.User != null)
                    {
                        App.User.AccountType = Domain.AccountType.Host;
                        upgradeSuccess = true;
                    }

                    if (upgradeSuccess)
                    {
                        _isUserHost = true;
                        UpdateCreateRunButton();

                        await DisplayAlert(
                            "🎉 Upgrade Successful!",
                            "Your account has been upgraded to Host!\n\n" +
                            "You can now create unlimited runs and access all Host features.",
                            "Awesome!");

                        await CheckHostStatusAndUpdateButton();
                    }
                    else
                    {
                        await DisplayAlert("Upgrade Failed",
                            "Payment was processed but account upgrade failed. Please contact support.",
                            "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Payment Failed",
                        "Your payment could not be processed. Please try again.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing in-app purchase: {ex.Message}");
                await DisplayAlert("Purchase Error",
                    "There was a problem processing your purchase. Please try again.",
                    "OK");
            }
        }

        private async Task ProcessUpgradeCode()
        {
            try
            {
                string code = await DisplayPromptAsync(
                    "Enter Upgrade Code",
                    "Enter your upgrade code to unlock Host features:",
                    "Activate",
                    "Cancel",
                    "Enter code here...",
                    maxLength: 20);

                if (string.IsNullOrWhiteSpace(code))
                {
                    return;
                }

                Debug.WriteLine($"Processing upgrade code: {code}");

                if (IsValidUpgradeCode(code.Trim()))
                {
                    bool upgradeSuccess = false;

                    if (_authService != null)
                    {
                        upgradeSuccess = await _authService.UpgradeToHostAccountAsync();
                    }
                    else if (App.User != null)
                    {
                        App.User.AccountType = Domain.AccountType.Host;
                        upgradeSuccess = true;
                    }

                    if (upgradeSuccess)
                    {
                        _isUserHost = true;
                        UpdateCreateRunButton();

                        await DisplayAlert(
                            "🎉 Code Activated!",
                            $"Upgrade code '{code}' has been activated!\n\n" +
                            "Your account is now upgraded to Host with full access to all features.",
                            "Great!");

                        await CheckHostStatusAndUpdateButton();
                    }
                    else
                    {
                        await DisplayAlert("Activation Failed",
                            "Valid code but upgrade failed. Please contact support.",
                            "OK");
                    }
                }
                else
                {
                    await DisplayAlert(
                        "Invalid Code",
                        $"The code '{code}' is not valid.\n\n" +
                        "Please check your code and try again, or contact support if you believe this is an error.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing upgrade code: {ex.Message}");
                await DisplayAlert("Code Error",
                    "Unable to process upgrade code. Please try again.",
                    "OK");
            }
        }

        private bool IsValidUpgradeCode(string code)
        {
            var validCodes = new[]
            {
                "HOSTME2024",
                "UPGRADE",
                "BASKETBALL",
                "HOOPS",
                "PREMIUM",
                "DEMO123",
                "TESTHOST",
                "FREEBETA"
            };

            return validCodes.Contains(code.ToUpperInvariant());
        }

        private async Task ShowContactSupport()
        {
            string action = await DisplayActionSheet(
                "Contact Support",
                "Cancel",
                null,
                "📧 Email Support",
                "📱 Call Support",
                "💬 Live Chat");

            switch (action)
            {
                case "📧 Email Support":
                    await DisplayAlert(
                        "Email Support",
                        "Send us an email at:\nsupport@ultimatehoopers.com\n\n" +
                        "Please include:\n" +
                        "• Your username\n" +
                        "• Request for Host upgrade\n" +
                        "• Any questions you have",
                        "OK");
                    break;
                case "📱 Call Support":
                    await DisplayAlert(
                        "Call Support",
                        "Call us at:\n1-800-HOOPERS\n(1-800-466-7377)\n\n" +
                        "Support Hours:\n" +
                        "Mon-Fri: 9AM-6PM EST\n" +
                        "Sat-Sun: 10AM-4PM EST",
                        "OK");
                    break;
                case "💬 Live Chat":
                    await DisplayAlert(
                        "Live Chat",
                        "Live chat coming soon!\n\n" +
                        "For now, please email us at support@ultimatehoopers.com",
                        "OK");
                    break;
            }
        }

        // Event handlers for run interactions
        private async void OnJoinButtonClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Join button clicked");

                if (sender is Button button && button.BindingContext is Run run)
                {
                    Debug.WriteLine($"Join button clicked for run: {run.Name}");

                    if (_viewModel?.JoinRunCommand?.CanExecute(run) == true)
                    {
                        _viewModel.JoinRunCommand.Execute(run);
                    }
                }
                else
                {
                    Debug.WriteLine("Could not get run from button binding context");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnJoinButtonClicked: {ex.Message}");
                await DisplayAlert("Error", $"Could not join run: {ex.Message}", "OK");
            }
        }

        private async void OnRunItemTapped(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Run item tapped");

                if (sender is Frame frame && frame.BindingContext is Run run)
                {
                    Debug.WriteLine($"Run item tapped: {run.Name}");

                    if (_viewModel?.ViewPrivateRunDetailsCommand?.CanExecute(run) == true)
                    {
                        _viewModel.ViewPrivateRunDetailsCommand.Execute(run);
                    }
                }
                else
                {
                    Debug.WriteLine("Could not get run from frame binding context");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnRunItemTapped: {ex.Message}");
                await DisplayAlert("Error", $"Could not view run details: {ex.Message}", "OK");
            }
        }

        private void SetupUI()
        {
            try
            {
                // Set default picker values if available
                if (DatePicker?.Items?.Count > 0 && DatePicker.SelectedItem == null)
                {
                    DatePicker.SelectedIndex = 0;
                }

                if (SkillLevelPicker?.Items?.Count > 0 && SkillLevelPicker.SelectedItem == null)
                {
                    SkillLevelPicker.SelectedIndex = 0;
                }

                // Register converter if needed
                if (Resources != null && !Resources.TryGetValue("InvertBoolConverter", out _))
                {
                    Resources.Add("InvertBoolConverter", new InvertBoolConverter());
                }

                // Wire up Create Run button
                if (CreateRunButton != null)
                {
                    CreateRunButton.Clicked += OnCreateRunButtonClicked;
                    Debug.WriteLine("Create Run button click handler attached");
                }
                else
                {
                    Debug.WriteLine("WARNING: CreateRunButton not found during SetupUI");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetupUI: {ex.Message}");
            }
        }

        // Navigation methods
        private async void OnBackClicked(object sender, TappedEventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating back: {ex.Message}");
            }
        }

        private async void OnHomeNavigationClicked(object sender, TappedEventArgs e)
        {
            try
            {
                Debug.WriteLine("FindRunsPage: OnHomeClicked - using DirectNavigationHelper");
                await DirectNavigationHelper.GoToHomePageAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindRunsPage: Error navigating to HomePage: {ex.Message}");
                await DisplayAlert("Navigation Error",
                    "Could not navigate to home page. Please try again or restart the app.",
                    "OK");
            }
        }

        private async void OnStatsNavigationClicked(object sender, TappedEventArgs e)
        {
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
            try
            {
                await DisplayAlert("Profile", "Profile page coming soon!", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in profile navigation: {ex.Message}");
            }
        }
    }
}