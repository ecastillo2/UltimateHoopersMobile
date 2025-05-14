using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;

namespace UltimateHoopers.Pages
{
    public partial class PostsPage : ContentPage
    {
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

            // Load posts when page appears
            await _viewModel.LoadPostsAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop any playing media
            StopAllMedia();
        }

        // Image post tap handler
        private void OnImagePostTapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is Post post)
            {
                ShowFullscreenImage(post);
            }
        }

        // Video post tap handler
        private void OnVideoPostTapped(object sender, EventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is Post post)
            {
                if (post.PostType?.Equals("video", StringComparison.OrdinalIgnoreCase) == true)
                {
                    PlayVideoInline(post, grid);
                }
            }
        }

        // Play video inline within the post
        private void PlayVideoInline(Post post, Grid postGrid)
        {
            // Store current post reference
            _currentMediaPost = post;

            try
            {
                // Get video container elements from the post grid's parent (the vertical stack layout)
                if (postGrid.Parent is VerticalStackLayout postContainer)
                {
                    // Find the grid containing the video player (3rd child of the post container)
                    var contentGrid = postContainer.Children.ElementAtOrDefault(1) as Grid;

                    if (contentGrid != null)
                    {
                        // Find the video player container within the content grid
                        var videoContainer = contentGrid.Children.FirstOrDefault(c => c is Grid g && g.ClassId == "videoPlayerContainer") as Grid;
                        var videoPlayer = videoContainer?.Children.FirstOrDefault(c => c is MediaElement) as MediaElement;

                        if (videoContainer != null && videoPlayer != null)
                        {
                            // Hide the thumbnail and show the video player
                            foreach (var child in contentGrid.Children)
                            {
                                if (child != videoContainer)
                                {
                                    child.IsVisible = false;
                                }
                            }

                            // Show video container and player
                            videoContainer.IsVisible = true;
                            videoPlayer.IsVisible = true;

                            // Set video source and play
                            videoPlayer.Source = MediaSource.FromUri(post.PostFileURL);
                            videoPlayer.Play();
                        }
                        else
                        {
                            // If we can't find the inline player, show fullscreen video instead
                            ShowFullscreenVideo(post);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If there's an error with inline play, fallback to fullscreen
                Console.WriteLine($"Error playing inline video: {ex.Message}");
                ShowFullscreenVideo(post);
            }
        }

        // Show fullscreen image viewer
        private void ShowFullscreenImage(Post post)
        {
            _currentMediaPost = post;

            // Reset scale and transform
            _currentScale = 1;
            fullscreenImage.Scale = 1;
            fullscreenImage.TranslationX = 0;
            fullscreenImage.TranslationY = 0;

            // Set image source
            fullscreenImage.Source = post.PostFileURL;

            // Show the fullscreen viewer with image
            fullscreenViewer.IsVisible = true;
            fullscreenImage.IsVisible = true;
            fullscreenVideoPlayer.IsVisible = false;

            // Stop the video player if it's playing
            if (fullscreenVideoPlayer.CurrentState == MediaElementState.Playing)
            {
                fullscreenVideoPlayer.Stop();
            }
        }

        // Show fullscreen video player
        private void ShowFullscreenVideo(Post post)
        {
            _currentMediaPost = post;

            // Set video source
            fullscreenVideoPlayer.Source = MediaSource.FromUri(post.PostFileURL);

            // Show the fullscreen viewer with video
            fullscreenViewer.IsVisible = true;
            fullscreenVideoPlayer.IsVisible = true;
            fullscreenImage.IsVisible = false;

            // Play the video
            fullscreenVideoPlayer.Play();
        }

        // Close fullscreen viewer
        private void CloseFullscreenViewer(object sender, EventArgs e)
        {
            // Stop the video if it's playing
            if (fullscreenVideoPlayer.CurrentState == MediaElementState.Playing)
            {
                fullscreenVideoPlayer.Stop();
            }

            // Hide the fullscreen viewer
            fullscreenViewer.IsVisible = false;
            fullscreenImage.IsVisible = false;
            fullscreenVideoPlayer.IsVisible = false;
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

        // Stop all media players
        private void StopAllMedia()
        {
            // Stop fullscreen video if playing
            if (fullscreenVideoPlayer != null && fullscreenVideoPlayer.CurrentState == MediaElementState.Playing)
            {
                fullscreenVideoPlayer.Stop();
            }

            // Stop any inline video players
            // This would need to find all MediaElements in the visual tree
            try
            {
                var mediaElements = FindVisualChildren<MediaElement>(this.Content);
                foreach (var mediaElement in mediaElements)
                {
                    if (mediaElement.CurrentState == MediaElementState.Playing)
                    {
                        mediaElement.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping media: {ex.Message}");
            }
        }

        // Helper method to find visual children of a certain type
        private IEnumerable<T> FindVisualChildren<T>(Element element) where T : Element
        {
            if (element == null)
                yield break;

            if (element is T typedElement)
                yield return typedElement;

            if (element is Layout<View> layout)
            {
                foreach (var child in layout.Children)
                {
                    foreach (var visualChild in FindVisualChildren<T>(child))
                    {
                        yield return visualChild;
                    }
                }
            }
            else if (element is ContentView contentView && contentView.Content != null)
            {
                foreach (var visualChild in FindVisualChildren<T>(contentView.Content))
                {
                    yield return visualChild;
                }
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