using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class EditProfilePage : ContentPage
    {
        // Mock user profile data - in a real app, this would come from a service
        private UserProfile _userProfile;

        // Flag to track if changes have been made
        private bool _profileChanged = false;

        // Selected image path
        private string _selectedImagePath;

        public EditProfilePage()
        {
            InitializeComponent();

            // Initialize with mock user data
            InitializeUserProfile();

            // Load data into the form
            LoadUserProfileData();
        }

        private void InitializeUserProfile()
        {
            // Mock user profile data
            _userProfile = new UserProfile
            {
                Username = "@yourUsername",
                FullName = "John Smith",
                Email = "john.smith@example.com",
                PhoneNumber = "(555) 123-4567",
                Position = "Point Guard",
                Height = "6'2\"",
                SkillLevel = "Intermediate",
                YearsPlaying = "10",
                City = "Atlanta",
                State = "Georgia",
                ShareLocation = true,
                PublicProfile = true,
                ShowStats = true,
                GameNotifications = true,
                ProfileImagePath = null
            };
        }

        private void LoadUserProfileData()
        {
            // Load data into form fields
            UsernameEntry.Text = App.User.Profile.UserName;
            FullNameEntry.Text = App.User.FirstName ;
            EmailEntry.Text = App.User.Email;
            PhoneEntry.Text = App.User.PhoneNumber;
            

            // Set pickers
            SetPickerValue(PositionPicker, App.User.Profile.Position);
            SetPickerValue(SkillLevelPicker, _userProfile.SkillLevel);

            HeightEntry.Text = App.User.Profile.Height;
            

            CityEntry.Text = _userProfile.City;
            StateEntry.Text = _userProfile.State;

            // Set switches
            ShareLocationSwitch.IsToggled = _userProfile.ShareLocation;
            PublicProfileSwitch.IsToggled = _userProfile.PublicProfile;
            ShowStatsSwitch.IsToggled = _userProfile.ShowStats;
            GameNotificationsSwitch.IsToggled = _userProfile.GameNotifications;

            // Load profile image if available
            if (!string.IsNullOrEmpty(App.User.Profile.ImageURL))
            {
                try
                {
                    ProfileImage.Source = App.User.Profile.ImageURL;
                    ProfileImage.IsVisible = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading profile image: {ex.Message}");
                }
            }
        }


        private void OnPositionPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            // When a position is selected, hide the placeholder
            PickerPlaceholder.IsVisible = (PositionPicker.SelectedIndex == -1);
        }

       

        private void SetPickerValue(Picker picker, string value)
        {
            for (int i = 0; i < picker.Items.Count; i++)
            {
                if (picker.Items[i] == value)
                {
                    picker.SelectedIndex = i;
                    break;
                }
            }
        }

        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Check if changes have been made
            if (_profileChanged)
            {
                bool saveChanges = await DisplayAlert("Save Changes",
                    "You have unsaved changes. Would you like to save them?",
                    "Save", "Discard");

                if (saveChanges)
                {
                    await SaveProfile();
                }
            }

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

        // Profile editing methods
        private async void OnChangePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                // Show options for selecting a photo
                string action = await DisplayActionSheet(
                    "Change Profile Photo",
                    "Cancel",
                    null,
                    "Take Photo",
                    "Choose from Gallery",
                    "Remove Photo");

                switch (action)
                {
                    case "Take Photo":
                        await TakePhoto();
                        break;

                    case "Choose from Gallery":
                        await PickPhoto();
                        break;

                    case "Remove Photo":
                        RemovePhoto();
                        break;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task TakePhoto()
        {
            if (!MediaPicker.IsCaptureSupported)
            {
                await DisplayAlert("Not Supported", "Camera is not supported on this device", "OK");
                return;
            }

            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    _selectedImagePath = photo.FullPath;

                    // Display the selected image
                    ProfileImage.Source = _selectedImagePath;
                    ProfileImage.IsVisible = true;

                    // Mark profile as changed
                    _profileChanged = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Camera error: {ex.Message}", "OK");
            }
        }

        private async Task PickPhoto()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select Profile Picture"
                });

                if (photo != null)
                {
                    _selectedImagePath = photo.FullPath;

                    // Display the selected image
                    ProfileImage.Source = _selectedImagePath;
                    ProfileImage.IsVisible = true;

                    // Mark profile as changed
                    _profileChanged = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Photo picker error: {ex.Message}", "OK");
            }
        }

        private void RemovePhoto()
        {
            // Hide the profile image and show default icon
            ProfileImage.Source = null;
            ProfileImage.IsVisible = false;
            _selectedImagePath = null;

            // Mark profile as changed
            _profileChanged = true;
        }

        private async void OnSaveChangesClicked(object sender, EventArgs e)
        {
            await SaveProfile();
        }

        private async Task SaveProfile()
        {
            try
            {
                // Show loading indicator
                var loadingIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = (Color)Application.Current.Resources["PrimaryColor"],
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                this.Content.IsEnabled = false;
                ((Grid)Content).Children.Add(loadingIndicator);

                // Simulate saving profile with delay
                await Task.Delay(1500);

                // Update the profile object with form values
                _userProfile.Username = UsernameEntry.Text;
                _userProfile.FullName = FullNameEntry.Text;
                _userProfile.Email = EmailEntry.Text;
                _userProfile.PhoneNumber = PhoneEntry.Text;

                _userProfile.Position = PositionPicker.SelectedItem?.ToString();
                _userProfile.Height = HeightEntry.Text;
                _userProfile.SkillLevel = SkillLevelPicker.SelectedItem?.ToString();
                
                _userProfile.City = CityEntry.Text;
                _userProfile.State = StateEntry.Text;

                _userProfile.ShareLocation = ShareLocationSwitch.IsToggled;
                _userProfile.PublicProfile = PublicProfileSwitch.IsToggled;
                _userProfile.ShowStats = ShowStatsSwitch.IsToggled;
                _userProfile.GameNotifications = GameNotificationsSwitch.IsToggled;

                // Update profile image
                _userProfile.ProfileImagePath = _selectedImagePath;

                // In a real app, save profile to a service or database
                // await _profileService.UpdateProfileAsync(_userProfile);

                // Reset changed flag
                _profileChanged = false;

                // Remove loading indicator
                ((Grid)Content).Children.Remove(loadingIndicator);
                this.Content.IsEnabled = true;

                // Show success message
                await DisplayAlert("Success", "Profile updated successfully", "OK");

                // Navigate back
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save profile: {ex.Message}", "OK");
                this.Content.IsEnabled = true;
            }
        }

        // Track changes in form fields
        protected override void OnAppearing()
        {
            base.OnAppearing();

            base.OnAppearing();

            // Set initial state - hide placeholder if there's already a selection
            PickerPlaceholder.IsVisible = (PositionPicker.SelectedIndex == -1);

            // Attach event handlers to track changes
            UsernameEntry.TextChanged += OnFormFieldChanged;
            FullNameEntry.TextChanged += OnFormFieldChanged;
            EmailEntry.TextChanged += OnFormFieldChanged;
            PhoneEntry.TextChanged += OnFormFieldChanged;

            PositionPicker.SelectedIndexChanged += OnFormFieldChanged;
            HeightEntry.TextChanged += OnFormFieldChanged;
            SkillLevelPicker.SelectedIndexChanged += OnFormFieldChanged;
           

            CityEntry.TextChanged += OnFormFieldChanged;
            StateEntry.TextChanged += OnFormFieldChanged;

            ShareLocationSwitch.Toggled += OnFormFieldChanged;
            PublicProfileSwitch.Toggled += OnFormFieldChanged;
            ShowStatsSwitch.Toggled += OnFormFieldChanged;
            GameNotificationsSwitch.Toggled += OnFormFieldChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Detach event handlers
            UsernameEntry.TextChanged -= OnFormFieldChanged;
            FullNameEntry.TextChanged -= OnFormFieldChanged;
            EmailEntry.TextChanged -= OnFormFieldChanged;
            PhoneEntry.TextChanged -= OnFormFieldChanged;

            PositionPicker.SelectedIndexChanged -= OnFormFieldChanged;
            HeightEntry.TextChanged -= OnFormFieldChanged;
            SkillLevelPicker.SelectedIndexChanged -= OnFormFieldChanged;
            

            CityEntry.TextChanged -= OnFormFieldChanged;
            StateEntry.TextChanged -= OnFormFieldChanged;

            ShareLocationSwitch.Toggled -= OnFormFieldChanged;
            PublicProfileSwitch.Toggled -= OnFormFieldChanged;
            ShowStatsSwitch.Toggled -= OnFormFieldChanged;
            GameNotificationsSwitch.Toggled -= OnFormFieldChanged;
        }

        private void OnFormFieldChanged(object sender, EventArgs e)
        {
            // Mark profile as changed when any form field is modified
            _profileChanged = true;
        }
    }

    // User profile model class
    public class UserProfile
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string Position { get; set; }
        public string Height { get; set; }
        public string SkillLevel { get; set; }
        public string YearsPlaying { get; set; }

        public string City { get; set; }
        public string State { get; set; }

        public bool ShareLocation { get; set; }
        public bool PublicProfile { get; set; }
        public bool ShowStats { get; set; }
        public bool GameNotifications { get; set; }

        public string ProfileImagePath { get; set; }
    }
}