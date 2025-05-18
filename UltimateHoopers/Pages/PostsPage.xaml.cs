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
using UltimateHoopers.Extensions;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Set up scrolled event handler for auto-play
                PostsCollectionView.Scrolled += OnCollectionViewScrolled;

                // Subscribe to MessagingCenter for full-screen image requests
                MessagingCenter.Subscribe<PostsViewModel, Post>(this, "ShowFullscreenImage", (sender, post) => {
                    ShowFullscreenImage(post);
                });

                // Check for visible videos after a short delay to ensure layout is done
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Task.Delay(500);
                    CheckVisibleVideos();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            try
            {
                // Remove event handler
                PostsCollectionView.Scrolled -= OnCollectionViewScrolled;

                // Unsubscribe from the MessagingCenter
                MessagingCenter.Unsubscribe<PostsViewModel, Post>(this, "ShowFullscreenImage");

                // Stop all videos
                var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");
                foreach (var element in videoElements)
                {
                    element.IsVisibleInViewport = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PostsPage.OnDisappearing: {ex.Message}");
            }
        }

        // Image loading event handlers
        private void OnImageLoaded(object sender, EventArgs e)
        {
            try
            {
                if (sender is Image image)
                {
                    // Find the parent grid containing this image
                    var parent = image.Parent;
                    while (parent != null && !(parent is Grid))
                    {
                        parent = parent.Parent;
                    }

                    if (parent is Grid grid)
                    {
                        // Find the loading indicator in the same grid
                        var loadingIndicator = grid.Children.OfType<ActivityIndicator>().FirstOrDefault();
                        if (loadingIndicator != null)
                        {
                            // Hide the loading indicator
                            loadingIndicator.IsVisible = false;
                            loadingIndicator.IsRunning = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling image loaded event: {ex.Message}");
            }
        }

        private void OnImageLoadingFailed(object sender, EventArgs e)
        {
            try
            {
                if (sender is Image image)
                {
                    // Set a fallback image
                    image.Source = "dotnet_bot.png";

                    // Find and hide the loading indicator
                    var parent = image.Parent;
                    while (parent != null && !(parent is Grid))
                    {
                        parent = parent.Parent;
                    }

                    if (parent is Grid grid)
                    {
                        var loadingIndicator = grid.Children.OfType<ActivityIndicator>().FirstOrDefault();
                        if (loadingIndicator != null)
                        {
                            loadingIndicator.IsVisible = false;
                            loadingIndicator.IsRunning = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling image loading failed event: {ex.Message}");
            }
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
            // Debounce technique to avoid too many checks
            MainThread.BeginInvokeOnMainThread(async () => {
                await Task.Delay(100); // Short delay to avoid excessive processing
                CheckVisibleVideos();
            });
        }

        // Method to check which videos are currently visible in the viewport
        private void CheckVisibleVideos()
        {
            try
            {
                if (PostsCollectionView == null) return;

                // Get visible elements directly rather than using binding context
                var videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo")
                    .ToList();

                // Nothing to do if no videos found
                if (!videoElements.Any())
                {
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Found {videoElements.Count} video elements in the collection");

                // For each video element, determine if it's visible
                foreach (var videoElement in videoElements)
                {
                    try
                    {
                        // Use the IsVisibleInViewport extension method
                        bool isElementVisible = videoElement.IsVisibleInViewport(PostsCollectionView, 0.3);

                        // Set visibility property which will trigger auto-play or pause
                        if (isElementVisible != videoElement.IsVisibleInViewport)
                        {
                            System.Diagnostics.Debug.WriteLine($"Setting video visibility to {isElementVisible}");
                            videoElement.IsVisibleInViewport = isElementVisible;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error checking video element visibility: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CheckVisibleVideos: {ex.Message}");
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
                try
                {
                    Debug.WriteLine($"Video full screen requested for {videoElement.Post.PostId}");
                    await NavigateToVideoPlayer(videoElement.Post);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error navigating to video player: {ex.Message}");
                }
            }
        }

        // Handle image post taps
        private void OnImagePostTapped(object sender, EventArgs e)
        {
            try
            {
                if (sender is Image image && image.BindingContext is Post post)
                {
                    Debug.WriteLine($"Tapped image post: {post.PostId}, URL: {post.PostFileURL}");

                    // Show full screen image viewer
                    ShowFullscreenImage(post);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnImagePostTapped: {ex.Message}");
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
}