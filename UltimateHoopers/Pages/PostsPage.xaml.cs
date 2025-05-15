using Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using Microsoft.Maui.ApplicationModel; // For MainThread
using System.Threading.Tasks;

namespace UltimateHoopers.Pages
{
    public partial class PostsPage : ContentPage
    {
        private List<object> _activePlayers = new List<object>(); // Define this properly
        private readonly PostsViewModel _viewModel;
        private Post _currentMediaPost;
        private double _currentScale = 1;
        private const double _minScale = 1;
        private const double _maxScale = 3;

        // Default constructor for XAML preview
        public PostsPage()
        {
            InitializeComponent();

            // Try to get view model from DI
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            var postService = serviceProvider.GetService<IPostService>();

            if (postService != null)
            {
                _viewModel = new PostsViewModel(postService);
            }
            else
            {
                // Fallback if service is not available through DI
                _viewModel = new PostsViewModel(new PostService());
            }

            // Set the binding context
            BindingContext = _viewModel;
        }

        // Constructor with dependency injection
        public PostsPage(IPostService postService)
        {
            InitializeComponent();

            // Create view model with injected service
            _viewModel = new PostsViewModel(postService ?? throw new ArgumentNullException(nameof(postService)));

            // Set the binding context
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Add verbose logging
                Console.WriteLine("PostsPage.OnAppearing - About to load posts");
                Debug.WriteLine("PostsPage.OnAppearing - About to load posts");

                // Check ViewModel availability
                if (_viewModel == null)
                {
                    Console.WriteLine("ERROR: ViewModel is null!");
                    await DisplayAlert("Error", "ViewModel is not initialized", "OK");
                    return;
                }

                // Load posts when page appears
                await _viewModel.LoadPostsAsync();

                // Verbose logging for post count
                Console.WriteLine($"PostsPage loaded {_viewModel.Posts.Count} posts");
                Debug.WriteLine($"PostsPage loaded {_viewModel.Posts.Count} posts");

                // If posts were loaded but aren't showing, check binding context
                if (_viewModel.Posts.Count > 0)
                {
                    Console.WriteLine("Posts were loaded but may not be displaying. Checking binding context...");
                    Console.WriteLine($"Current binding context: {BindingContext}");

                    // Apply direct image source conversion for each post if needed
                    ProcessPostImages();

                    // Force refresh the binding
                    BindingContext = null;
                    BindingContext = _viewModel;
                    Console.WriteLine("Binding context reset. This should refresh the UI.");
                }
                else
                {
                    Console.WriteLine("No posts were loaded from the service.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
                Debug.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Could not load posts: {ex.Message}", "OK");
            }
        }

        // Helper method to process post images directly
        private void ProcessPostImages()
        {
            try
            {
                foreach (var post in _viewModel.Posts)
                {
                    // Make sure PostFileURL is a valid URI
                    if (!string.IsNullOrWhiteSpace(post.PostFileURL))
                    {
                        try
                        {
                            // Ensure URL has a protocol (http or https)
                            if (!post.PostFileURL.StartsWith("http://") && !post.PostFileURL.StartsWith("https://"))
                            {
                                // Add https protocol if missing
                                post.PostFileURL = "https://" + post.PostFileURL.TrimStart('/');
                                Console.WriteLine($"Fixed URL by adding protocol: {post.PostFileURL}");
                            }

                            // Test creating a URI object to validate
                            var uri = new Uri(post.PostFileURL);
                            Console.WriteLine($"Valid URI: {uri}");
                        }
                        catch (UriFormatException ex)
                        {
                            Console.WriteLine($"Invalid URL format: {post.PostFileURL}, Error: {ex.Message}");
                        }
                    }

                    // Do the same for ThumbnailUrl
                    if (!string.IsNullOrWhiteSpace(post.ThumbnailUrl))
                    {
                        try
                        {
                            if (!post.ThumbnailUrl.StartsWith("http://") && !post.ThumbnailUrl.StartsWith("https://"))
                            {
                                post.ThumbnailUrl = "https://" + post.ThumbnailUrl.TrimStart('/');
                                Console.WriteLine($"Fixed thumbnail URL: {post.ThumbnailUrl}");
                            }

                            // Test creating a URI object
                            var uri = new Uri(post.ThumbnailUrl);
                            Console.WriteLine($"Valid thumbnail URI: {uri}");
                        }
                        catch (UriFormatException ex)
                        {
                            Console.WriteLine($"Invalid thumbnail URL: {post.ThumbnailUrl}, Error: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPostImages: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop all videos when page disappears
            StopAllVideos();
        }

        // Image post tap handler
        private void OnImagePostTapped(object sender, EventArgs e)
        {
            try
            {
                if (sender is Image image && image.BindingContext is Post post)
                {
                    Console.WriteLine($"Tapped image post: {post.PostId}, URL: {post.PostFileURL}");

                    // Show full screen image viewer
                    ShowFullscreenImage(post);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnImagePostTapped: {ex.Message}");
            }
        }

        // Video post tap handler
        private async void OnVideoPostTapped(object sender, EventArgs e)
        {
            try
            {
                var element = sender as VisualElement;
                if (element != null && element.BindingContext is Post post)
                {
                    Debug.WriteLine($"Tapped video post: {post.PostId}, URL: {post.PostFileURL}");

                    if (string.IsNullOrWhiteSpace(post.PostFileURL))
                    {
                        await DisplayAlert("Error", "Video URL is not available", "OK");
                        return;
                    }

                    // Simply open the video player as a modal page
                    await Navigation.PushModalAsync(new VideoPlayerPage(post));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnVideoPostTapped: {ex.Message}");
                await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
            }
        }

        // Handler for video thumbnail tap events
        private async void OnVideoThumbnailTapped(object sender, EventArgs e)
        {
            try
            {
                var element = sender as VisualElement;
                if (element != null && element.BindingContext is Post post)
                {
                    Debug.WriteLine($"Tapped video thumbnail: {post.PostId}, URL: {post.PostFileURL}");

                    if (string.IsNullOrWhiteSpace(post.PostFileURL))
                    {
                        await DisplayAlert("Error", "Video URL is not available", "OK");
                        return;
                    }

                    // Navigate to the video player page
                    await Navigation.PushModalAsync(new VideoPlayerPage(post));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnVideoThumbnailTapped: {ex.Message}");
                await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
            }
        }

        // Removed FindVideoPlayer method since InlineVideoPlayer doesn't exist

        // Fallback method to navigate to dedicated video player page
        private async void NavigateToVideoPlayer(Post post)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(post.PostFileURL))
                {
                    await DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                // Display options for video
                string action = await DisplayActionSheet(
                    "View Video",
                    "Cancel",
                    null,
                    "Play in App",
                    "Open in Browser");

                switch (action)
                {
                    case "Play in App":
                        // Navigate to video player
                        await Navigation.PushModalAsync(new VideoPlayerPage(post));
                        break;

                    case "Open in Browser":
                        // Open in external browser
                        await Launcher.OpenAsync(new Uri(post.PostFileURL));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to video player: {ex.Message}");
                await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
            }
        }

        // Play video directly in the post using WebView
        private async Task DisplayInlineVideo(Post post)
        {
            try
            {
                string videoUrl = post.PostFileURL;
                if (string.IsNullOrWhiteSpace(videoUrl))
                {
                    await DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                Console.WriteLine($"Displaying video options for URL: {videoUrl}");

                // Create an action sheet to let the user choose how to view the video
                string action = await DisplayActionSheet(
                    "View Video",
                    "Cancel",
                    null,
                    "Play Video",
                    "Open in Browser");

                switch (action)
                {
                    case "Play Video":
                        Console.WriteLine("User chose to play video");
                        // Navigate to simplified video player
                        await Navigation.PushModalAsync(new VideoPlayerPage(post));
                        break;

                    case "Open in Browser":
                        Console.WriteLine("User chose to open in browser");
                        // Open in browser using the device's default browser
                        await OpenVideoInBrowser(post);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying video: {ex.Message}");
                await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
            }
        }

        // Open video in the device's browser
        private async Task OpenVideoInBrowser(Post post)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(post.PostFileURL))
                {
                    await DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                Console.WriteLine($"Opening in browser: {post.PostFileURL}");

                // Use the device's browser to open the video
                await Launcher.OpenAsync(new Uri(post.PostFileURL));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
                await DisplayAlert("Error", $"Could not open browser: {ex.Message}", "OK");
            }
        }

        // Show fullscreen image viewer
        private void ShowFullscreenImage(Post post)
        {
            try
            {
                _currentMediaPost = post;

                // Reset scale and transform
                _currentScale = 1;
                fullscreenImage.Scale = 1;
                fullscreenImage.TranslationX = 0;
                fullscreenImage.TranslationY = 0;

                Console.WriteLine($"Setting fullscreen image source: {post.PostFileURL}");

                // Set image source - FIXED: Convert string URL to ImageSource
                if (!string.IsNullOrEmpty(post.PostFileURL))
                {
                    try
                    {
                        // Use ImageSource.FromUri to properly load the image from a URL
                        fullscreenImage.Source = ImageSource.FromUri(new Uri(post.PostFileURL));
                        Console.WriteLine($"Image source set using ImageSource.FromUri: {post.PostFileURL}");
                    }
                    catch (UriFormatException ex)
                    {
                        Console.WriteLine($"Invalid URI format: {ex.Message}");
                        fullscreenImage.Source = "dotnet_bot.png"; // Fallback to a default image
                    }
                }
                else
                {
                    Console.WriteLine("PostFileURL is null or empty");
                    fullscreenImage.Source = "dotnet_bot.png"; // Fallback to a default image
                }

                // Show the fullscreen viewer with image
                fullscreenViewer.IsVisible = true;
                fullscreenImage.IsVisible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing fullscreen image: {ex.Message}");
                DisplayAlert("Error", $"Could not display image: {ex.Message}", "OK");
            }
        }

        // Close fullscreen viewer
        private void CloseFullscreenViewer(object sender, EventArgs e)
        {
            // Hide the fullscreen viewer
            fullscreenViewer.IsVisible = false;
            fullscreenImage.IsVisible = false;
            _currentMediaPost = null;
        }

        // Fullscreen image tap handler (to close it)
        private void OnFullscreenImageTapped(object sender, EventArgs e)
        {
            // Only close if not zoomed in
            if (_currentScale <= 1.1)
            {
                CloseFullscreenViewer(sender, e);
            }
        }

        // Pinch to zoom handler
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                // Store the current scale when the gesture begins
                _currentScale = fullscreenImage.Scale;
            }
            else if (e.Status == GestureStatus.Running)
            {
                // Calculate the new scale based on the pinch gesture
                double newScale = _currentScale * e.Scale;

                // Apply scale limits
                newScale = Math.Max(_minScale, Math.Min(_maxScale, newScale));

                // Apply the new scale to the image
                fullscreenImage.Scale = newScale;
            }
            else if (e.Status == GestureStatus.Completed)
            {
                // Save the final scale
                _currentScale = fullscreenImage.Scale;
            }
        }

        // Pan gesture handler for moving zoomed image
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            // Only allow panning when zoomed in
            if (_currentScale <= 1)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Apply the translation
                    fullscreenImage.TranslationX += e.TotalX;
                    fullscreenImage.TranslationY += e.TotalY;
                    break;
            }
        }

        private void StopAllVideos()
        {
            // Since we removed InlineVideoPlayer references, we'll just clear the list
            _activePlayers.Clear();
        }

        // Add this to track active players
        private void OnVideoStarted(object sender, EventArgs e)
        {
            if (sender != null && !_activePlayers.Contains(sender))
            {
                _activePlayers.Add(sender);
            }
        }

        // Handler for Image loaded event
        private void OnImageLoaded(object sender, EventArgs e)
        {
            // This method is referenced in the XAML but was missing in the code-behind
            // It can be used to perform actions when an image finishes loading
            if (sender is Image image)
            {
                // You could add logic here such as:
                // - Hide a loading indicator
                // - Adjust layout based on loaded image dimensions
                // - Log successful image loads
                Debug.WriteLine($"Image loaded: {image.Source}");
            }
        }

        // Navigation handlers
        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//HomePage");
        }

        private async void OnCreatePostClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Create Post", "Create Post feature coming soon!", "OK");
        }

        private async void OnActivityClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Activity", "Activity feature coming soon!", "OK");
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Profile", "Profile feature coming soon!", "OK");
        }
    }
}