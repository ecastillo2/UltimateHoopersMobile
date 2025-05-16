using Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using Microsoft.Maui.ApplicationModel; // For MainThread
using System.Threading.Tasks;
using UltimateHoopers.Controls;
using System.Linq;

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

        // Dictionary to track visible videos
        private Dictionary<string, bool> _visibleVideos = new Dictionary<string, bool>();

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

                // Set up scrolled event handler for auto-play
                PostsCollectionView.Scrolled += OnCollectionViewScrolled;

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

                    // Initial check for visible videos after layout
                    MainThread.BeginInvokeOnMainThread(async () => {
                        // Small delay to ensure UI is ready
                        await Task.Delay(500);
                        CheckVisibleVideos();
                    });
                }
                else
                {
                    Console.WriteLine("No posts were loaded from the service.");
                }

                // Subscribe to mute events
                SubscribeToMuteEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
                Debug.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Could not load posts: {ex.Message}", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Clean up event handlers
            PostsCollectionView.Scrolled -= OnCollectionViewScrolled;

            // Stop all videos when page disappears
            StopAllVideos();

            // Unsubscribe from mute events
            UnsubscribeFromMuteEvents();
        }

        // Subscribe to AutoPlayVideoElement mute events
        private void SubscribeToMuteEvents()
        {
            // Find all video elements in the collection view
            var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");

            foreach (var videoElement in videoElements)
            {
                videoElement.MuteStateChanged += OnVideoMuteStateChanged;
            }
        }

        // Unsubscribe from events to prevent memory leaks
        private void UnsubscribeFromMuteEvents()
        {
            var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");

            foreach (var videoElement in videoElements)
            {
                videoElement.MuteStateChanged -= OnVideoMuteStateChanged;
            }
        }

        // Handle mute state changes
        private void OnVideoMuteStateChanged(object sender, bool isMuted)
        {
            if (sender is AutoPlayVideoElement videoElement)
            {
                // Find the parent grid of the video element
                var parent = videoElement.Parent;
                while (parent != null && !(parent is Grid))
                {
                    parent = parent.Parent;
                }

                if (parent is Grid grid)
                {
                    // Find the volume button frame
                    var volumeFrame = grid.Children.OfType<Frame>()
                        .FirstOrDefault(f => f.StyleId == "VolumeButton" || f.ClassId == "VolumeButton");

                    if (volumeFrame != null && volumeFrame.Content is Label volumeIcon)
                    {
                        // Update the icon based on current mute state
                        MainThread.BeginInvokeOnMainThread(() => {
                            volumeIcon.Text = isMuted ? "🔇" : "🔊";
                        });
                    }
                }
            }
        }

        // Handle volume button tap
        private void OnVolumeButtonTapped(object sender, EventArgs e)
        {
            try
            {
                if (sender is Element element)
                {
                    // Find the parent layout containing both the volume button and video element
                    var parentLayout = element.Parent;
                    while (parentLayout != null && !(parentLayout is Grid))
                    {
                        parentLayout = parentLayout.Parent;
                    }

                    if (parentLayout is Grid grid)
                    {
                        // Find the AutoPlayVideoElement within the same grid
                        var videoElement = grid.Children.OfType<AutoPlayVideoElement>().FirstOrDefault();
                        if (videoElement != null)
                        {
                            // Toggle mute state
                            videoElement.ToggleMute();

                            // Find and update the volume icon
                            var volumeFrame = grid.Children.OfType<Frame>()
                                .FirstOrDefault(f => f.StyleId == "VolumeButton" || f.ClassId == "VolumeButton");

                            if (volumeFrame != null && volumeFrame.Content is Label volumeIcon)
                            {
                                // Update the icon based on mute state
                                volumeIcon.Text = videoElement.IsMuted ? "🔇" : "🔊";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling volume: {ex.Message}");
            }
        }

        // Event handler for CollectionView scrolling
        private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            // Check for visible videos during scrolling
            CheckVisibleVideos();
        }

        // Method to check which videos are currently visible in the viewport
        private void CheckVisibleVideos()
        {
            try
            {
                if (PostsCollectionView == null) return;

                // Get the visible items in the collection view
                var visibleItems = GetVisibleItems(PostsCollectionView);

                // Track which videos should be playing
                Dictionary<string, bool> newVisibleVideos = new Dictionary<string, bool>();

                foreach (var item in visibleItems)
                {
                    if (item is Post post && post.PostType?.ToLower() == "video" && !string.IsNullOrEmpty(post.PostId))
                    {
                        // Add to tracking dictionary
                        newVisibleVideos[post.PostId] = true;

                        // Instead of trying to find the control by post ID (which is unreliable),
                        // we'll find all video elements and update them if their binding context matches

                        var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");
                        foreach (var videoElement in videoElements)
                        {
                            if (videoElement.Post is Post videoPost && videoPost.PostId == post.PostId)
                            {
                                // Set visibility property to true (which will trigger auto-play)
                                videoElement.IsVisibleInViewport = true;
                                break; // Found the matching element, no need to check others
                            }
                        }
                    }
                }

                // Check for videos that were visible but are no longer visible
                foreach (var postId in _visibleVideos.Keys)
                {
                    if (!newVisibleVideos.ContainsKey(postId))
                    {
                        // This video is no longer visible
                        var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");
                        foreach (var videoElement in videoElements)
                        {
                            if (videoElement.Post is Post videoPost && videoPost.PostId == postId)
                            {
                                // Set visibility property to false (which will trigger pause)
                                videoElement.IsVisibleInViewport = false;
                                break; // Found the matching element, no need to check others
                            }
                        }
                    }
                }

                // Update tracking dictionary
                _visibleVideos = newVisibleVideos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking visible videos: {ex.Message}");
            }
        }

        // Find video elements by post ID
        private AutoPlayVideoElement FindAutoPlayVideoElement(string postId)
        {
            try
            {
                // Get the container for the post
                var container = PostsCollectionView.FindVisualChildrenByName<VerticalStackLayout>("postContainer")
                    .FirstOrDefault(c => {
                        if (c.BindingContext is Post post)
                            return post.PostId == postId;
                        return false;
                    });

                if (container == null)
                    return null;

                // Find the AutoPlayVideoElement in the container
                return container.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo").FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding video element: {ex.Message}");
                return null;
            }
        }

        // Helper method to get visible items in the CollectionView
        private IEnumerable<object> GetVisibleItems(CollectionView collectionView)
        {
            try
            {
                // Get all items
                var allItems = collectionView.ItemsSource?.Cast<object>().ToList();
                if (allItems == null || !allItems.Any())
                    return Enumerable.Empty<object>();

                // Since we don't have direct access to scroll position or visible indices in MAUI's CollectionView,
                // we'll take a different approach to determine visible items

                // Find all post containers in the visual tree
                var allContainers = collectionView.FindVisualChildrenByName<VerticalStackLayout>("postContainer")
                    .Where(c => c.BindingContext != null)
                    .ToList();

                if (!allContainers.Any())
                {
                    // Fallback: return first few items since we can't determine visibility
                    int approximateVisibleItemCount = Math.Max(3, (int)(collectionView.Height / 500));
                    return allItems.Take(Math.Min(approximateVisibleItemCount, allItems.Count));
                }

                // Try to estimate which containers are visible
                // We'll use the container's IsVisible property and Bounds
                var visibleItems = new List<object>();

                foreach (var container in allContainers)
                {
                    // Basic check - if IsVisible is true and container has reasonable height
                    if (container.IsVisible && container.Height > 0)
                    {
                        visibleItems.Add(container.BindingContext);
                    }
                }

                // If we couldn't find any visible items, return first few as fallback
                if (!visibleItems.Any())
                {
                    int approximateVisibleItemCount = Math.Max(3, (int)(collectionView.Height / 500));
                    return allItems.Take(Math.Min(approximateVisibleItemCount, allItems.Count));
                }

                return visibleItems;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting visible items: {ex.Message}");

                // Last resort fallback - return first few items
                try
                {
                    var allItems = collectionView.ItemsSource?.Cast<object>().ToList();
                    if (allItems != null && allItems.Any())
                    {
                        return allItems.Take(Math.Min(3, allItems.Count));
                    }
                }
                catch
                {
                    // Ignore any nested exceptions
                }

                return Enumerable.Empty<object>();
            }
        }

        // Handle size changes for post containers to recalculate visibility
        private void OnPostContainerSizeChanged(object sender, EventArgs e)
        {
            // When a post container changes size, recalculate visible videos
            MainThread.BeginInvokeOnMainThread(() => {
                CheckVisibleVideos();
            });
        }

        // Handle video full-screen requests
        private async void OnVideoFullScreenRequested(object sender, EventArgs e)
        {
            if (sender is AutoPlayVideoElement videoElement && videoElement.Post != null)
            {
                await NavigateToVideoPlayer(videoElement.Post);
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

        // Navigate to a video player page
        private async Task NavigateToVideoPlayer(Post post)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(post.PostFileURL))
                {
                    await DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                // Navigate to the video player page
                await Navigation.PushModalAsync(new VideoPlayerPage(post));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to video player: {ex.Message}");
                await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
            }
        }

        private void StopAllVideos()
        {
            // Find all AutoPlayVideoElement controls
            var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");

            // Pause each video
            foreach (var element in videoElements)
            {
                element.IsVisibleInViewport = false;
            }

            // Clear tracking dictionary
            _visibleVideos.Clear();
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

    // Extension methods to find child elements by name
    public static class VisualElementExtensions
    {
        public static IEnumerable<T> FindVisualChildrenByName<T>(this Element element, string name = null) where T : VisualElement
        {
            var results = new List<T>();

            if (element is T foundElement && (string.IsNullOrEmpty(name) || foundElement.StyleId == name))
            {
                results.Add(foundElement);
            }

            foreach (var child in element.LogicalChildren)
            {
                if (child is Element visualChild)
                {
                    results.AddRange(FindVisualChildrenByName<T>(visualChild, name));
                }
            }

            return results;
        }
    }
}