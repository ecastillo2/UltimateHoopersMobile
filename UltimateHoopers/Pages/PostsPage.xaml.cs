using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel; // For MainThread
using System.Threading.Tasks;
using UltimateHoopers.Controls;
using System.Linq;
using UltimateHoopers.Extensions;
using UltimateHoopers.ViewModels;
using Domain;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class PostsPage : ContentPage
    {
        private readonly PostsViewModel _viewModel;
        private Post _currentMediaPost;
        private double _currentScale = 1;
        private const double _minScale = 1;
        private const double _maxScale = 3;

        // Dictionary to track visible videos
        private Dictionary<string, bool> _visibleVideos = new Dictionary<string, bool>();

        // Flag to track if initialization is complete
        private bool _isInitialized = false;

        // Default constructor for XAML preview
        public PostsPage()
        {
            try
            {
                InitializeComponent();

                // Try to get view model from DI
                var serviceProvider = MauiProgram.CreateMauiApp().Services;
                var postService = serviceProvider?.GetService<IPostService>();

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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PostsPage constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't rethrow - continue to show the page even with errors
            }
        }

        // Constructor with dependency injection
        public PostsPage(IPostService postService)
        {
            try
            {
                InitializeComponent();

                // Create view model with injected service
                _viewModel = new PostsViewModel(postService ?? throw new ArgumentNullException(nameof(postService)));

                // Set the binding context
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PostsPage DI constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't rethrow - continue to show the page even with errors
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Set up scrolled event handler for auto-play but only once
                if (!_isInitialized)
                {
                    if (PostsCollectionView != null)
                    {
                        PostsCollectionView.Scrolled += OnCollectionViewScrolled;
                    }

                    // Subscribe to MessagingCenter for full-screen image requests
                    MessagingCenter.Subscribe<PostsViewModel, Post>(this, "ShowFullscreenImage", (sender, post) => {
                        ShowFullscreenImage(post);
                    });

                    _isInitialized = true;
                }

                // Check for visible videos after a short delay to ensure layout is done
                MainThread.BeginInvokeOnMainThread(async () => {
                    try
                    {
                        await Task.Delay(500);
                        CheckVisibleVideos();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error checking visible videos: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            try
            {
                // Don't remove the event handler permanently, just pause video playback
                StopAllVideos();
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
                try
                {
                    await Task.Delay(100); // Short delay to avoid excessive processing
                    CheckVisibleVideos();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in scroll handler: {ex.Message}");
                }
            });
        }

        // Method to check which videos are currently visible in the viewport
        private void CheckVisibleVideos()
        {
            try
            {
                if (PostsCollectionView == null) return;

                // Get visible elements but don't crash if nothing found
                var videoElements = new List<AutoPlayVideoElement>();

                try
                {
                    videoElements = PostsCollectionView.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo").ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error finding video elements: {ex.Message}");
                    return;
                }

                // Nothing to do if no videos found
                if (videoElements.Count == 0)
                {
                    Debug.WriteLine("No video elements found in the collection");
                    return;
                }

                Debug.WriteLine($"Found {videoElements.Count} video elements in the collection");

                // For each video element, determine if it's visible
                foreach (var videoElement in videoElements)
                {
                    try
                    {
                        // Safely check if video element is in viewport
                        bool isElementVisible = IsElementVisible(videoElement, PostsCollectionView);

                        // Set visibility property which will trigger auto-play or pause
                        if (isElementVisible != videoElement.IsVisibleInViewport)
                        {
                            Debug.WriteLine($"Setting video visibility to {isElementVisible}");
                            videoElement.IsVisibleInViewport = isElementVisible;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error checking video element visibility: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CheckVisibleVideos: {ex.Message}");
            }
        }

        // Safer method to check element visibility
        private bool IsElementVisible(VisualElement element, CollectionView collectionView)
        {
            try
            {
                if (element == null || collectionView == null)
                    return false;

                // Get the bounds of the element and collection view
                double elementY = element.Y;
                double elementHeight = element.Height;

                if (elementHeight <= 0)
                    return false;

                // Get parent's position to estimate absolute position
                var parent = element.Parent;
                int nestLevel = 0;
                while (parent != null && parent != collectionView && nestLevel < 10)
                {
                    if (parent is VisualElement visualParent)
                    {
                        elementY += visualParent.Y;
                    }
                    parent = parent.Parent;
                    nestLevel++; // Prevent infinite loops
                }

                // Approximate the viewport position
                double collectionHeight = collectionView.Height;
                double collectionY = collectionView.Y;
                double scrollY = 0; // Cannot reliably get scroll position in MAUI CollectionView

                // Calculate visibility percentage based on rough approximation
                double visibleTop = Math.Max(scrollY, elementY);
                double visibleBottom = Math.Min(scrollY + collectionHeight, elementY + elementHeight);
                double visibleHeight = Math.Max(0, visibleBottom - visibleTop);
                double visibilityPercentage = visibleHeight / elementHeight;

                // Consider an element visible if at least 30% is in view
                return visibilityPercentage >= 0.3;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in IsElementVisible: {ex.Message}");
                return false;
            }
        }

        // Handle post container size changes to recalculate visibility
        private void OnPostContainerSizeChanged(object sender, EventArgs e)
        {
            // When a post container changes size, recalculate visible videos
            MainThread.BeginInvokeOnMainThread(() => {
                try
                {
                    CheckVisibleVideos();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in OnPostContainerSizeChanged: {ex.Message}");
                }
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
                if (fullscreenImage != null)
                {
                    fullscreenImage.Scale = 1;
                    fullscreenImage.TranslationX = 0;
                    fullscreenImage.TranslationY = 0;
                }

                Debug.WriteLine($"Setting fullscreen image source: {post.PostFileURL}");

                if (fullscreenImage != null && !string.IsNullOrEmpty(post.PostFileURL))
                {
                    try
                    {
                        // Use ImageSource.FromUri to properly load the image from a URL
                        fullscreenImage.Source = ImageSource.FromUri(new Uri(post.PostFileURL));
                        Debug.WriteLine($"Image source set using ImageSource.FromUri: {post.PostFileURL}");
                    }
                    catch (UriFormatException ex)
                    {
                        Debug.WriteLine($"Invalid URI format: {ex.Message}");
                        fullscreenImage.Source = "dotnet_bot.png"; // Fallback to a default image
                    }
                }
                else
                {
                    Debug.WriteLine("PostFileURL is null or empty or fullscreenImage is null");
                    if (fullscreenImage != null)
                    {
                        fullscreenImage.Source = "dotnet_bot.png"; // Fallback to a default image
                    }
                }

                // Show the fullscreen viewer with image
                if (fullscreenViewer != null)
                {
                    fullscreenViewer.IsVisible = true;

                    if (fullscreenImage != null)
                    {
                        fullscreenImage.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing fullscreen image: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Error", $"Could not display image: {ex.Message}", "OK");
                });
            }
        }

        // Close fullscreen viewer
        private void CloseFullscreenViewer(object sender, EventArgs e)
        {
            try
            {
                // Hide the fullscreen viewer
                if (fullscreenViewer != null)
                {
                    fullscreenViewer.IsVisible = false;

                    if (fullscreenImage != null)
                    {
                        fullscreenImage.IsVisible = false;
                    }
                }

                _currentMediaPost = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing fullscreen viewer: {ex.Message}");
            }
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
            if (fullscreenImage == null) return;

            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling pinch: {ex.Message}");
            }
        }

        // Pan gesture handler for moving zoomed image
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (fullscreenImage == null) return;

            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling pan: {ex.Message}");
            }
        }

        // Navigate to a video player page
        private async Task NavigateToVideoPlayer(Post post)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(post?.PostFileURL))
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
            try
            {
                var videoElements = PostsCollectionView?.FindVisualChildrenByName<AutoPlayVideoElement>("AutoPlayVideo");

                // Pause each video
                if (videoElements != null)
                {
                    foreach (var element in videoElements)
                    {
                        element.IsVisibleInViewport = false;
                    }
                }

                // Clear tracking dictionary
                _visibleVideos.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping videos: {ex.Message}");
            }
        }

        // Navigation handlers
        private async void OnHomeClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//HomePage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to home: {ex.Message}");
                // Fallback if Shell navigation fails
                await Navigation.PushAsync(new HomePage());
            }
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