using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using UltimateHoopers.Controls;

namespace UltimateHoopers.Pages
{
    public partial class VideoPlayerPage : ContentPage
    {
        private readonly Domain.Post _post;
        private bool _isVideoLoaded = false;
        private bool _isMuted = false; // Default to unmuted for better fullscreen experience

        // Default constructor for design time and XAML previews
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        // Constructor with post parameter
        public VideoPlayerPage(Domain.Post post)
        {
            InitializeComponent();
            _post = post ?? throw new ArgumentNullException(nameof(post));

            try
            {
                // Set binding context for social interactions
                BindingContext = _post;

                // Set caption if available
                if (!string.IsNullOrWhiteSpace(_post.Caption))
                {
                    captionLabel.Text = _post.Caption;
                }
                else if (!string.IsNullOrWhiteSpace(_post.UserName))
                {
                    captionLabel.Text = "Video by " + _post.UserName;
                }
                else
                {
                    captionLabel.Text = "Video";
                }

                // Set thumbnail image - prioritize ThumbnailUrl if available, otherwise use PostFileURL
                SetThumbnailImage();

                // Initially show the fallback grid with thumbnail and play button
                fallbackGrid.IsVisible = true;
                videoWebView.IsVisible = false;
                loadingIndicator.IsVisible = false;

                // Show volume button with appropriate icon
                UpdateVolumeButtonIcon();

                // Debug output
                Debug.WriteLine($"VideoPlayerPage created for post: {_post.PostId}");
                Debug.WriteLine($"PostFileURL: {_post.PostFileURL}");
                Debug.WriteLine($"ThumbnailUrl: {_post.ThumbnailUrl ?? "Not available"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VideoPlayerPage constructor: {ex.Message}");
                DisplayAlert("Error", $"Error loading video: {ex.Message}", "OK");
            }
        }

        private void SetThumbnailImage()
        {
            try
            {
                // Choose the thumbnail source - prefer ThumbnailUrl if available
                string thumbnailUrl = !string.IsNullOrWhiteSpace(_post.ThumbnailUrl) ?
                    _post.ThumbnailUrl : _post.PostFileURL;

                if (!string.IsNullOrWhiteSpace(thumbnailUrl))
                {
                    Debug.WriteLine($"Setting thumbnail: {thumbnailUrl}");

                    // Make sure the URL has a valid protocol
                    if (!thumbnailUrl.StartsWith("http://") && !thumbnailUrl.StartsWith("https://"))
                    {
                        thumbnailUrl = "https://" + thumbnailUrl.TrimStart('/');
                        Debug.WriteLine($"Fixed thumbnail URL with protocol: {thumbnailUrl}");
                    }

                    try
                    {
                        thumbnailImage.Source = ImageSource.FromUri(new Uri(thumbnailUrl));
                    }
                    catch (UriFormatException ex)
                    {
                        Debug.WriteLine($"Invalid thumbnail URI: {ex.Message}");
                        // Fall back to default image
                        thumbnailImage.Source = "dotnet_bot.png";
                    }
                }
                else
                {
                    thumbnailImage.Source = "dotnet_bot.png";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting thumbnail: {ex.Message}");
                thumbnailImage.Source = "dotnet_bot.png"; // Fallback to default image
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Automatically start playing when the page appears
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Small delay to ensure the UI is ready
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    OnPlayButtonTapped(this, EventArgs.Empty);
                    return false; // Don't repeat
                });
            });
        }

        // Handle the play button tap event
        private void OnPlayButtonTapped(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Play button tapped");

                // Only proceed if we have a valid video URL
                if (string.IsNullOrWhiteSpace(_post?.PostFileURL))
                {
                    Debug.WriteLine("PostFileURL is empty");
                    DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                // Validate URL format
                if (!Uri.TryCreate(_post.PostFileURL, UriKind.Absolute, out Uri videoUri))
                {
                    Debug.WriteLine($"Invalid video URL format: {_post.PostFileURL}");
                    DisplayAlert("Error", "Invalid video URL format", "OK");
                    return;
                }

                // Show loading indicator, hide play button
                playButtonFrame.IsVisible = false;
                loadingIndicator.IsVisible = true;

                string videoUrl = _post.PostFileURL;
                Debug.WriteLine($"Loading video URL: {videoUrl}");

                // Ensure URL has a protocol
                if (!videoUrl.StartsWith("http://") && !videoUrl.StartsWith("https://"))
                {
                    videoUrl = "https://" + videoUrl.TrimStart('/');
                    Debug.WriteLine($"Fixed video URL with protocol: {videoUrl}");
                }

                // Create an enhanced HTML wrapper for the video
                string videoHtml = GetVideoHtml(videoUrl);

                // Load the HTML into the WebView
                videoWebView.Source = new HtmlWebViewSource
                {
                    Html = videoHtml
                };

                // Show the WebView
                videoWebView.IsVisible = true;
                Debug.WriteLine("WebView is now visible with video source set");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing video: {ex.Message}");
                DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");

                // Reset UI states
                playButtonFrame.IsVisible = true;
                loadingIndicator.IsVisible = false;
            }
        }

        // Toggle mute state
        private void OnVolumeButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Toggle mute state
                _isMuted = !_isMuted;
                Debug.WriteLine($"Volume button clicked. New mute state: {_isMuted}");

                // Update the volume button icon
                UpdateVolumeButtonIcon();

                // Update the video mute state if it's loaded
                if (_isVideoLoaded && videoWebView.Handler != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        try
                        {
                            // Execute the JavaScript to toggle muting with enhanced error checking
                            string result = await videoWebView.EvaluateJavaScriptAsync(
                                $"console.log('Setting muted to {(_isMuted ? "true" : "false")}');" +
                                $"var video = document.getElementById('videoPlayer');" +
                                $"if(video) {{ " +
                                $"  video.muted = {(_isMuted ? "true" : "false")}; " +
                                $"  console.log('Video muted set to: ' + video.muted); " +
                                $"  video.volume = 1.0; " + // Set volume to maximum
                                $"  console.log('Video volume set to: ' + video.volume); " +
                                $"  video.muted.toString(); " +
                                $"}} else {{ " +
                                $"  console.log('Video element not found'); " +
                                $"  'Video element not found'; " +
                                $"}}");

                            Debug.WriteLine($"JavaScript result: {result}");
                        }
                        catch (Exception jsEx)
                        {
                            Debug.WriteLine($"JavaScript evaluation error: {jsEx.Message}");

                            // If JavaScript fails, try reloading the video with the new mute state
                            Debug.WriteLine("Reloading video with new mute state after JavaScript error");

                            if (!string.IsNullOrEmpty(_post?.PostFileURL))
                            {
                                videoWebView.Source = new HtmlWebViewSource { Html = GetVideoHtml(_post.PostFileURL) };
                            }
                        }
                    });
                }
                else if (!_isVideoLoaded && fallbackGrid.IsVisible)
                {
                    // If video isn't loaded yet, we'll apply the mute state when it loads
                    Debug.WriteLine("Video not loaded yet, mute state will be applied when it loads");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling mute: {ex.Message}");
            }
        }

        // Update the volume button icon based on current mute state
        private void UpdateVolumeButtonIcon()
        {
            if (volumeButton != null && volumeIcon != null)
            {
                volumeIcon.Text = _isMuted ? "🔇" : "🔊";
                Debug.WriteLine($"Volume icon updated to: {volumeIcon.Text}");
            }
        }

        // HTML template for video player
        private string GetVideoHtml(string videoUrl)
        {
            // Add cache busting parameter
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

            // Log the video URL being used
            Debug.WriteLine($"Using video URL with cache busting: {urlWithCacheBusting}");
            Debug.WriteLine($"Initial muted state: {_isMuted}");

            return @"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {
            margin: 0;
            padding: 0;
            background-color: #000;
            height: 100vh;
            width: 100vw;
            display: flex;
            justify-content: center;
            align-items: center;
            overflow: hidden;
        }
        .video-container {
            width: 100%;
            height: 100%;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        video {
            width: 100%;
            height: 100%;
            max-height: 100vh;
            object-fit: contain;
        }
    </style>
</head>
<body>
    <div class='video-container'>
        <video id='videoPlayer' controls autoplay playsinline" + (_isMuted ? " muted" : "") + @">
            <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
            Your browser does not support HTML5 video.
        </video>
    </div>
    <script>
        // Wait for document to be fully loaded
        document.addEventListener('DOMContentLoaded', function() {
            console.log('Video player HTML initialized');
            var video = document.getElementById('videoPlayer');
            
            if (video) {
                // Configure audio settings
                video.muted = " + (_isMuted ? "true" : "false") + @";
                video.volume = 1.0; // Set volume to maximum
                
                // Log settings for debugging
                console.log('Initial muted state: ' + video.muted);
                console.log('Initial volume: ' + video.volume);
                
                // Monitor volume changes for debugging
                video.addEventListener('volumechange', function() {
                    console.log('Volume changed. Muted: ' + video.muted + ', Volume: ' + video.volume);
                });
                
                // Setup other event listeners
                video.addEventListener('canplay', function() {
                    console.log('Video can play now');
                    window.location.href = 'maui-callback://videoCanPlay';
                });
                
                video.addEventListener('error', function(e) {
                    console.log('Video error: ' + (e.target.error ? e.target.error.code : 'unknown'));
                    window.location.href = 'maui-callback://videoError';
                });
                
                video.addEventListener('playing', function() {
                    console.log('Video now playing');
                    window.location.href = 'maui-callback://videoPlaying';
                });
                
                // Force load the video
                video.load();
                
                // Try to autoplay (this may be blocked by browser policies)
                setTimeout(function() {
                    console.log('Attempting to play video...');
                    var playPromise = video.play();
                    
                    if (playPromise !== undefined) {
                        playPromise.then(function() {
                            console.log('Autoplay started successfully');
                        }).catch(function(error) {
                            console.log('Autoplay prevented: ' + error);
                        });
                    }
                }, 500);
            } else {
                console.error('Video element not found!');
            }
        });
    </script>
</body>
</html>";
        }

        // WebView navigation event handlers
        private void VideoWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("maui-callback://"))
            {
                e.Cancel = true; // Cancel the navigation

                if (e.Url == "maui-callback://videoCanPlay")
                {
                    Debug.WriteLine("Video can play callback received");
                    // You could add additional handling here if needed
                }
                else if (e.Url == "maui-callback://videoError")
                {
                    Debug.WriteLine("Video error callback received");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        bool openExternal = await DisplayAlert(
                            "Playback Issue",
                            "The video couldn't be played in the app. Would you like to open it in your browser?",
                            "Open in Browser",
                            "Cancel");

                        if (openExternal && _post != null && !string.IsNullOrWhiteSpace(_post.PostFileURL))
                        {
                            await Launcher.OpenAsync(new Uri(_post.PostFileURL));
                        }
                        else
                        {
                            // Reset UI if user cancels
                            fallbackGrid.IsVisible = true;
                            playButtonFrame.IsVisible = true;
                            loadingIndicator.IsVisible = false;
                        }
                    });
                }
                else if (e.Url == "maui-callback://videoPlaying")
                {
                    Debug.WriteLine("Video playing callback received");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Hide fallback once playing
                        fallbackGrid.IsVisible = false;
                        loadingIndicator.IsVisible = false;

                        // Short delay before applying mute state again to ensure video is fully initialized
                        await Task.Delay(300);

                        if (_isVideoLoaded && videoWebView.Handler != null)
                        {
                            try
                            {
                                // Reapply mute state to ensure it takes effect
                                string result = await videoWebView.EvaluateJavaScriptAsync(
                                    $"console.log('Reapplying muted state to {(_isMuted ? "true" : "false")}');" +
                                    $"var video = document.getElementById('videoPlayer');" +
                                    $"if(video) {{ " +
                                    $"  video.volume = 1.0; " + // Ensure volume is at maximum
                                    $"  video.muted = {(_isMuted ? "true" : "false")}; " +
                                    $"  console.log('Muted state reapplied: ' + video.muted); " +
                                    $"  console.log('Volume is: ' + video.volume); " +
                                    $"  true; " +
                                    $"}} else {{ " +
                                    $"  console.log('Video element not found'); " +
                                    $"  false; " +
                                    $"}}");

                                Debug.WriteLine($"Reapplied mute state, result: {result}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error reapplying mute state: {ex.Message}");
                            }
                        }
                    });
                }
            }
        }

        private void VideoWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Success)
            {
                Debug.WriteLine("WebView loaded successfully");
                _isVideoLoaded = true;

                // Add a delay before hiding the loading indicator
                // This gives the video player time to initialize
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1000);

                    // If the fallback is still visible, hide it
                    if (fallbackGrid.IsVisible)
                    {
                        fallbackGrid.IsVisible = false;
                        loadingIndicator.IsVisible = false;
                    }

                    // Apply mute state after successful load
                    try
                    {
                        string result = await videoWebView.EvaluateJavaScriptAsync(
                            $"console.log('Setting muted after navigation to {(_isMuted ? "true" : "false")}');" +
                            $"var video = document.getElementById('videoPlayer');" +
                            $"if(video) {{ " +
                            $"  video.volume = 1.0; " +
                            $"  video.muted = {(_isMuted ? "true" : "false")}; " +
                            $"  console.log('Muted: ' + video.muted + ', Volume: ' + video.volume); " +
                            $"  true; " +
                            $"}} else {{ " +
                            $"  console.log('Video element not found after navigation'); " +
                            $"  false; " +
                            $"}}");

                        Debug.WriteLine($"Applied mute state after navigation, result: {result}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error applying mute state after navigation: {ex.Message}");
                    }
                });
            }
            else
            {
                Debug.WriteLine($"WebView navigation failed: {e.Result}");
                _isVideoLoaded = false;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Show fallback on error
                    fallbackGrid.IsVisible = true;
                    playButtonFrame.IsVisible = true;
                    loadingIndicator.IsVisible = false;
                });
            }
        }

        // Close button handler
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            try
            {
                // Clean up resources if needed
                Debug.WriteLine("Closing video player");

                // Use Navigation to go back/dismiss the modal
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing video player: {ex.Message}");
                // Fallback if navigation fails
                if (Application.Current != null)
                {
                    Application.Current.MainPage = new AppShell();
                }
            }
        }

        private void OnLikeClicked(object sender, EventArgs e)
        {
            if (_post != null)
            {
                // Toggle like state
                _post.LikedPost = !(_post.LikedPost ?? false);

                // Update like count
                if (_post.LikedPost == true)
                {
                    _post.Likes = (_post.Likes ?? 0) + 1;
                }
                else
                {
                    _post.Likes = Math.Max(0, (_post.Likes ?? 0) - 1);
                }

                // Force UI update
                OnPropertyChanged(nameof(_post.LikedPost));

                // You would call your API here to update the like status
                // await _postService.LikePostAsync(_post.PostId, _post.LikedPost ?? false);
            }
        }

        private async void OnCommentsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Comments", "Comments feature coming soon!", "OK");
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Share", "Share feature coming soon!", "OK");
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_post != null)
            {
                // Toggle save state
                _post.SavedPost = !(_post.SavedPost ?? false);

                // Force UI update
                OnPropertyChanged(nameof(_post.SavedPost));

                // You would call your API here to update the save status
                // await _postService.SavePostAsync(_post.PostId, _post.SavedPost ?? false);

                // Show confirmation
                string message = _post.SavedPost == true ? "Post saved to collection" : "Post removed from collection";
                await DisplayAlert("Saved", message, "OK");
            }
        }
    }
}