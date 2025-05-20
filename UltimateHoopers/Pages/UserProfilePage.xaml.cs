using Domain;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserProfilePage : ContentPage
    {
        private readonly IPostService _postService;
        private readonly IProfileService _profileService;
        private List<Post> _userPosts = new List<Post>();

        // Current tab state
        private bool _isPostsTabActive = true;

        public UserProfilePage()
        {
            InitializeComponent();

            // Get services from DI
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            _postService = serviceProvider.GetService<IPostService>() ?? new PostService();
            _profileService = serviceProvider.GetService<IProfileService>() ?? new ProfileService();

            // Load data when page appears
            this.Appearing += OnPageAppearing;
        }

        private async void OnPageAppearing(object sender, EventArgs e)
        {
            try
            {
                // Show loading indicator (could add an activity indicator)

                // Load user profile data
                await LoadUserProfileData();

                // Load user posts
                await LoadUserPosts();

                // Hide loading indicator
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile data: {ex.Message}");
                await DisplayAlert("Error", "Failed to load profile data", "OK");
            }
        }

        private async Task LoadUserProfileData()
        {
            try
            {
                // Check if user is logged in
                if (App.User == null || App.User.Profile == null)
                {
                    Debug.WriteLine("User not logged in or profile not available");
                    return;
                }

                // Set the username
                UsernameLabel.Text = App.User.UserName ?? "username";

                // Set display name
                DisplayNameLabel.Text = string.IsNullOrEmpty(App.User.FirstName) || string.IsNullOrEmpty(App.User.LastName)
                    ? App.User.UserName
                    : $"{App.User.FirstName} {App.User.LastName}";

                // Set bio if available
                BioLabel.Text = App.User.Profile.Bio ?? "No bio available";

                // Load profile image
                LoadProfileImage();

                // Try to get more detailed profile info from the service
                try
                {
                    var profile = await _profileService.GetProfileByIdAsync(App.User.Profile.ProfileId);
                    if (profile != null)
                    {
                        // Update UI with profile data
                        if (profile.GameStatistics != null)
                        {
                            // Set posts count - using TotalGames as a placeholder for now
                            PostsCountLabel.Text = profile.GameStatistics.TotalGames.ToString();

                            // Set followers/following if available
                            FollowersCountLabel.Text = profile.FollowersCount ?? "0";
                            FollowingCountLabel.Text = profile.FollowingCount ?? "0";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching detailed profile: {ex.Message}");
                    // Continue with basic profile display
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadUserProfileData: {ex.Message}");
                throw;
            }
        }

        private void LoadProfileImage()
        {
            try
            {
                // Check if user has a profile image URL
                if (App.User != null && App.User.Profile != null &&
                    !string.IsNullOrEmpty(App.User.Profile.ImageURL))
                {
                    // Create a proper URI for the image source
                    if (Uri.TryCreate(App.User.Profile.ImageURL, UriKind.Absolute, out Uri imageUri))
                    {
                        // Set the image source
                        UserProfileImage.Source = ImageSource.FromUri(imageUri);

                        // Show the image, hide the default icon
                        DefaultProfileIcon.IsVisible = false;
                        UserProfileImage.IsVisible = true;

                        // Fallback in case image fails to load
                        MainThread.BeginInvokeOnMainThread(async () => {
                            await Task.Delay(1000);

                            // If the image doesn't have valid dimensions after the delay, it likely failed to load
                            if (UserProfileImage.Width <= 0 || UserProfileImage.Height <= 0)
                            {
                                Debug.WriteLine("Profile image appears to have failed loading (no dimensions)");
                                DefaultProfileIcon.IsVisible = true;
                                UserProfileImage.IsVisible = false;
                            }
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid image URI: {App.User.Profile.ImageURL}");
                        DefaultProfileIcon.IsVisible = true;
                        UserProfileImage.IsVisible = false;
                    }
                }
                else
                {
                    // No profile image available
                    DefaultProfileIcon.IsVisible = true;
                    UserProfileImage.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile image: {ex.Message}");
                DefaultProfileIcon.IsVisible = true;
                UserProfileImage.IsVisible = false;
            }
        }

        private async Task LoadUserPosts()
        {
            try
            {
                // Clear existing posts grid
                PostsGrid.Clear();

                // Fetch user's posts
                // In a real app, you would filter by user ID
                var allPosts = await _postService.GetPostsAsync();

                // Filter by this user's ID if available
                _userPosts = App.User?.UserId != null
                    ? allPosts.Where(p => p.UserId == App.User.UserId).ToList()
                    : allPosts.Take(9).ToList(); // For demo purposes, just show some posts

                // Show empty state if no posts
                if (_userPosts == null || _userPosts.Count == 0)
                {
                    EmptyPostsState.IsVisible = true;
                    return;
                }

                // Hide empty state
                EmptyPostsState.IsVisible = false;

                // Create the posts grid
                CreatePostsGrid();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user posts: {ex.Message}");
                throw;
            }
        }

        private void CreatePostsGrid()
        {
            try
            {
                // Clear any existing children
                PostsGrid.Children.Clear();

                // Calculate the number of rows needed (3 posts per row)
                int rowCount = (_userPosts.Count + 2) / 3; // Ceiling division

                // Ensure we have enough rows in the grid
                while (PostsGrid.RowDefinitions.Count < rowCount)
                {
                    PostsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120) });
                }

                // Add posts to grid
                for (int i = 0; i < _userPosts.Count; i++)
                {
                    var post = _userPosts[i];

                    // Calculate grid position
                    int row = i / 3;
                    int col = i % 3;

                    // Create the post thumbnail
                    var frame = CreatePostThumbnail(post);

                    // Add to grid
                    PostsGrid.Add(frame, col, row);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating posts grid: {ex.Message}");
            }
        }

        private Frame CreatePostThumbnail(Post post)
        {
            // Create a frame to hold the post thumbnail
            var frame = new Frame
            {
                Padding = 0,
                CornerRadius = 0,
                IsClippedToBounds = true,
                BackgroundColor = Colors.LightGray,
                HeightRequest = 120
            };

            // Create a grid to hold the image and icon
            var grid = new Grid();

            // Create the image
            var image = new Image
            {
                Aspect = Aspect.AspectFill,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // Set the image source based on post type
            string imageUrl = !string.IsNullOrEmpty(post.ThumbnailUrl)
                ? post.ThumbnailUrl
                : post.PostFileURL;

            // Ensure URL has protocol
            if (!string.IsNullOrEmpty(imageUrl))
            {
                if (!imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://"))
                {
                    imageUrl = "https://" + imageUrl.TrimStart('/');
                }

                try
                {
                    image.Source = ImageSource.FromUri(new Uri(imageUrl));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting image source: {ex.Message}");
                    // Use a placeholder for failed images
                    image.Source = "dotnet_bot.png";
                }
            }
            else
            {
                // Use a placeholder if no image URL
                image.Source = "dotnet_bot.png";
            }

            // Add the image to the grid
            grid.Add(image);

            // For video posts, add a video icon indicator
            if (post.PostType?.ToLower() == "video")
            {
                var videoIndicator = new Label
                {
                    Text = "▶️",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 5, 5, 0)
                };

                grid.Add(videoIndicator);
            }

            // Set the frame's content
            frame.Content = grid;

            // Add tap gesture to open the post
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnPostThumbnailTapped(post);
            frame.GestureRecognizers.Add(tapGesture);

            return frame;
        }

        private async void OnPostThumbnailTapped(Post post)
        {
            try
            {
                // In a real app, navigate to post details page
                if (post.PostType?.ToLower() == "video")
                {
                    // For videos, open the video player page
                    await Navigation.PushModalAsync(new VideoPlayerPage(post));
                }
                else
                {
                    // For images or other posts, display a placeholder alert for now
                    await DisplayAlert("Post Details", $"Viewing post: {post.Caption}", "Close");

                    // In a real app, you would navigate to a post details page:
                    // await Navigation.PushAsync(new PostDetailsPage(post));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling post tap: {ex.Message}");
                await DisplayAlert("Error", "Could not open post", "OK");
            }
        }

        // Tab navigation
        private void OnPostsTabClicked(object sender, EventArgs e)
        {
            if (!_isPostsTabActive)
            {
                // Switch to posts tab
                _isPostsTabActive = true;

                // Update indicators
                PostsTabIndicator.IsVisible = true;
                TaggedTabIndicator.IsVisible = false;

                // Update content visibility
                PostsGrid.IsVisible = true;
                TaggedGrid.IsVisible = false;

                // Check if posts are empty
                EmptyPostsState.IsVisible = _userPosts == null || _userPosts.Count == 0;
            }
        }

        private void OnTaggedTabClicked(object sender, EventArgs e)
        {
            if (_isPostsTabActive)
            {
                // Switch to tagged tab
                _isPostsTabActive = false;

                // Update indicators
                PostsTabIndicator.IsVisible = false;
                TaggedTabIndicator.IsVisible = true;

                // Update content visibility
                PostsGrid.IsVisible = false;
                TaggedGrid.IsVisible = true;
                EmptyPostsState.IsVisible = false;
            }
        }

        // Event handlers for buttons
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Settings", "Settings page would open here", "OK");
        }

        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditProfilePage());
        }

        private async void OnShareFirstPostClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Create Post", "Create Post feature coming soon!", "OK");
        }
    }

    // Extension method to clear grid children
    public static class GridExtensions
    {
        public static void Clear(this Grid grid)
        {
            grid.Children.Clear();
        }
    }
}

// 3. Modify the OnProfileClicked method in PostsPage.xaml.cs
// to navigate to the new UserProfilePage

