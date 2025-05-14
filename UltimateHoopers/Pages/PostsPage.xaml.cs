using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltimateHoopers.Services;
using UltimateHoopers.ViewModels;
using Microsoft.Maui.ApplicationModel;

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
            if (sender is Grid videoGrid && videoGrid.BindingContext is Post post)
            {
                if (post.PostType?.Equals("video", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // We can't use the video player yet, so show a message
                    ShowFullscreenImage(post); // Fallback to showing image

                    // Show a message about video
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Video", "Video player is currently under maintenance. Coming soon!", "OK");
                    });
                }
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