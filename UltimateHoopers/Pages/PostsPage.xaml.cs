using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using Microsoft.Maui.ApplicationModel; // For MainThread
using Grid = Microsoft.Maui.Controls.Grid; // Explicitly specify which Grid to use

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

            try
            {
                // Load posts when page appears
                await _viewModel.LoadPostsAsync();
                Console.WriteLine($"PostsPage loaded {_viewModel.Posts.Count} posts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostsPage.OnAppearing: {ex.Message}");
                await DisplayAlert("Error", $"Could not load posts: {ex.Message}", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
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
                if (sender is Grid videoGrid && videoGrid.BindingContext is Post post)
                {
                    Console.WriteLine($"Tapped video post: {post.PostId}, URL: {post.PostFileURL}");

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnVideoPostTapped: {ex.Message}");
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

                // Set image source
                fullscreenImage.Source = post.PostFileURL;

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
