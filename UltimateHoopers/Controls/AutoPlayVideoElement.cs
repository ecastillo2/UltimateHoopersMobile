using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UltimateHoopers.Controls
{
    /// <summary>
    /// A custom control for auto-playing videos during scrolling with volume control
    /// </summary>
    public class AutoPlayVideoElement : ContentView
    {
        // Video source URL
        public static readonly BindableProperty VideoUrlProperty =
            BindableProperty.Create(nameof(VideoUrl), typeof(string), typeof(AutoPlayVideoElement), null,
                propertyChanged: OnVideoUrlChanged);

        // Thumbnail URL to display before playing
        public static readonly BindableProperty ThumbnailUrlProperty =
            BindableProperty.Create(nameof(ThumbnailUrl), typeof(string), typeof(AutoPlayVideoElement), null);

        // Whether the video is currently visible in the viewport
        public static readonly BindableProperty IsVisibleInViewportProperty =
            BindableProperty.Create(nameof(IsVisibleInViewport), typeof(bool), typeof(AutoPlayVideoElement), false,
                propertyChanged: OnIsVisibleInViewportChanged);

        // Mute state of the video
        public static readonly BindableProperty IsMutedProperty =
            BindableProperty.Create(nameof(IsMuted), typeof(bool), typeof(AutoPlayVideoElement), true,
                propertyChanged: OnIsMutedChanged);

        // Post object for reference
        public static readonly BindableProperty PostProperty =
            BindableProperty.Create(nameof(Post), typeof(Domain.Post), typeof(AutoPlayVideoElement), null);

        // Video player reference
        private WebView _videoPlayer;

        // Thumbnail image
        private Image _thumbnailImage;

        // Play button overlay
        private Frame _playButtonOverlay;

        // Loading indicator
        private ActivityIndicator _loadingIndicator;

        // Track if video is loaded
        private bool _isVideoLoaded = false;

        // Track if user has manually paused
        private bool _isManuallyPaused = false;

        // Get/set video URL
        public string VideoUrl
        {
            get => (string)GetValue(VideoUrlProperty);
            set => SetValue(VideoUrlProperty, value);
        }

        // Get/set thumbnail URL
        public string ThumbnailUrl
        {
            get => (string)GetValue(ThumbnailUrlProperty);
            set => SetValue(ThumbnailUrlProperty, value);
        }

        // Get/set visibility in viewport
        public bool IsVisibleInViewport
        {
            get => (bool)GetValue(IsVisibleInViewportProperty);
            set => SetValue(IsVisibleInViewportProperty, value);
        }

        // Get/set muted state
        public bool IsMuted
        {
            get => (bool)GetValue(IsMutedProperty);
            set => SetValue(IsMutedProperty, value);
        }

        // Get/set post object
        public Domain.Post Post
        {
            get => (Domain.Post)GetValue(PostProperty);
            set => SetValue(PostProperty, value);
        }

        // Event to notify when tapping for full-screen playback
        public event EventHandler<EventArgs> FullScreenRequested;

        // Event to notify when the mute state changes
        public event EventHandler<bool> MuteStateChanged;

        // Constructor
        public AutoPlayVideoElement()
        {
            // Create a grid to hold all elements
            var grid = new Grid
            {
                HeightRequest = 400
            };

            // Create thumbnail image
            _thumbnailImage = new Image
            {
                Aspect = Aspect.AspectFill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            // Create WebView for video playing
            _videoPlayer = new WebView
            {
                IsVisible = false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Black,
                Opacity = 0.0 // Start invisible and fade in
            };

            // Add WebView navigated event
            _videoPlayer.Navigated += VideoPlayer_Navigated;

            // Loading indicator
            _loadingIndicator = new ActivityIndicator
            {
                IsRunning = false,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Color = Colors.White
            };

            // Play button
            _playButtonOverlay = new Frame
            {
                BackgroundColor = new Color(0, 0, 0, 0.5f),
                CornerRadius = 30,
                HeightRequest = 60,
                WidthRequest = 60,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HasShadow = false,
                Content = new Label
                {
                    Text = "▶️",
                    FontSize = 32,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.White
                }
            };

            // Add tap gesture to entire control for entering full screen
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnTapped;
            grid.GestureRecognizers.Add(tapGesture);

            // Add all elements to the grid
            grid.Add(_thumbnailImage);
            grid.Add(_videoPlayer);
            grid.Add(_loadingIndicator);
            grid.Add(_playButtonOverlay);

            // Set content
            Content = grid;
        }

        // Handle taps on the video
        private void OnTapped(object sender, EventArgs e)
        {
            // Trigger the full screen event to let the page handle it
            FullScreenRequested?.Invoke(this, e);
        }

        // Called when the video URL changes
        private static void OnVideoUrlChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AutoPlayVideoElement)bindable;
            if (newValue is string url && !string.IsNullOrEmpty(url))
            {
                control.SetThumbnail();
            }
        }

        // Called when viewport visibility changes
        private static void OnIsVisibleInViewportChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (AutoPlayVideoElement)bindable;
            if (newValue is bool isVisible)
            {
                if (isVisible)
                {
                    control.AutoPlay();
                }
                else
                {
                    control.Pause();
                }
            }
        }

        // Called when mute state changes
        private static void OnIsMutedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AutoPlayVideoElement videoElement)
            {
                videoElement.UpdateMuteState();
            }
        }

        // Method to update the mute state of the video
        private void UpdateMuteState()
        {
            try
            {
                // If video is not loaded yet, we'll apply the mute state when it loads
                if (!_isVideoLoaded)
                {
                    Debug.WriteLine("Video not loaded yet, mute state will be applied when loaded");

                    // If video is playing, reload it with the new mute state
                    if (_videoPlayer.IsVisible && !string.IsNullOrEmpty(VideoUrl))
                    {
                        Debug.WriteLine("Reloading video with new mute state");
                        _videoPlayer.Source = new HtmlWebViewSource { Html = GetVideoHtml(VideoUrl) };
                    }

                    // Notify about mute state change
                    MuteStateChanged?.Invoke(this, IsMuted);
                    return;
                }

                // If the WebView handler isn't available, we can't update the video
                if (_videoPlayer == null || _videoPlayer.Handler == null)
                {
                    Debug.WriteLine("WebView handler not available");
                    return;
                }

                // Execute JavaScript to update the mute state
                MainThread.BeginInvokeOnMainThread(async () => {
                    try
                    {
                        // Log the mute state change
                        Debug.WriteLine($"Updating video mute state to {(IsMuted ? "muted" : "unmuted")}");

                        // Use JavaScript to set the muted property
                        string jsResult = await _videoPlayer.EvaluateJavaScriptAsync(
                            $"console.log('Setting muted to {(IsMuted ? "true" : "false")}');" +
                            $"var video = document.getElementById('videoPlayer');" +
                            $"if(video) {{ " +
                            $"  video.muted = {(IsMuted ? "true" : "false")}; " +
                            $"  console.log('Video muted state set to ' + video.muted); " +
                            $"  video.muted.toString(); " +
                            $"}} else {{ " +
                            $"  console.log('Video element not found'); " +
                            $"  'Video element not found'; " +
                            $"}}");

                        // Log the result of the JavaScript execution
                        Debug.WriteLine($"JavaScript result: {jsResult}");

                        // Notify about mute state change
                        MuteStateChanged?.Invoke(this, IsMuted);
                    }
                    catch (Exception jsEx)
                    {
                        Debug.WriteLine($"JavaScript evaluation error updating mute state: {jsEx.Message}");

                        // If JavaScript fails, reload the video with the new mute state
                        if (!string.IsNullOrEmpty(VideoUrl))
                        {
                            Debug.WriteLine("Reloading video with new mute state after JavaScript error");
                            _videoPlayer.Source = new HtmlWebViewSource { Html = GetVideoHtml(VideoUrl) };
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating mute state: {ex.Message}");
            }
        }

        // Set thumbnail image
        private void SetThumbnail()
        {
            try
            {
                // Use thumbnail URL if available, otherwise use first frame of video
                string thumbnailSource = !string.IsNullOrEmpty(ThumbnailUrl) ? ThumbnailUrl : VideoUrl;

                if (!string.IsNullOrEmpty(thumbnailSource))
                {
                    // Ensure URL has proper protocol
                    if (!thumbnailSource.StartsWith("http://") && !thumbnailSource.StartsWith("https://"))
                    {
                        thumbnailSource = "https://" + thumbnailSource.TrimStart('/');
                    }

                    // Set image source
                    _thumbnailImage.Source = ImageSource.FromUri(new Uri(thumbnailSource));
                }
                else
                {
                    // Fallback to placeholder
                    _thumbnailImage.Source = "dotnet_bot.png";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting thumbnail: {ex.Message}");
                _thumbnailImage.Source = "dotnet_bot.png";
            }
        }

        // WebView loaded event handler
        // Update the VideoPlayer_Navigated method to properly handle the video loading completion
        private void VideoPlayer_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Success)
            {
                Debug.WriteLine("WebView navigated successfully");

                // Set flag
                _isVideoLoaded = true;

                // Instead of waiting for JavaScript callbacks which might be unreliable,
                // we'll use a short delay and then show the video directly
                MainThread.BeginInvokeOnMainThread(async () => {
                    try
                    {
                        // Short delay to give the video time to initialize
                        await Task.Delay(500);

                        // Hide loading indicator
                        _loadingIndicator.IsVisible = false;
                        _loadingIndicator.IsRunning = false;

                        // Show the video with a fade effect
                        await _videoPlayer.FadeTo(1.0, 300);

                        // Hide the play button
                        _playButtonOverlay.IsVisible = false;

                        // Apply the mute state directly through JavaScript
                        await ApplyMuteState();

                        // Optional: Hide thumbnail after video appears
                        // Only hide if we're sure the video is visible
                        if (_videoPlayer.Opacity > 0.9)
                        {
                            _thumbnailImage.IsVisible = false;
                        }

                        Debug.WriteLine("Video playback started");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error showing video after navigation: {ex.Message}");
                        ShowErrorState();
                    }
                });
            }
            else
            {
                Debug.WriteLine($"WebView navigation failed: {e.Result}");
                _isVideoLoaded = false;

                // Show error state
                ShowErrorState();
            }
        }

        private void ShowErrorState()
        {
            MainThread.BeginInvokeOnMainThread(() => {
                _loadingIndicator.IsVisible = false;
                _loadingIndicator.IsRunning = false;
                _playButtonOverlay.IsVisible = true;
                _thumbnailImage.IsVisible = true;
                _videoPlayer.IsVisible = false;
            });
        }

        private async Task ApplyMuteState()
        {
            try
            {
                if (_videoPlayer.Handler != null)
                {
                    string jsResult = await _videoPlayer.EvaluateJavaScriptAsync(
                        $"var video = document.getElementById('videoPlayer');" +
                        $"if(video) {{ " +
                        $"  video.muted = {(IsMuted ? "true" : "false")}; " +
                        $"  video.volume = 1.0; " +
                        $"  'Mute applied'; " +
                        $"}} else {{ " +
                        $"  'Video element not found'; " +
                        $"}}");

                    Debug.WriteLine($"Mute JS result: {jsResult}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying mute state: {ex.Message}");
            }
        }

        // Auto play the video when in viewport
        public void AutoPlay()
        {
            if (_isManuallyPaused || string.IsNullOrEmpty(VideoUrl)) return;

            try
            {
                // Show loading state
                _loadingIndicator.IsVisible = true;
                _loadingIndicator.IsRunning = true;
                _playButtonOverlay.IsVisible = false;

                // Make sure the thumbnail is visible during loading
                _thumbnailImage.IsVisible = true;

                // Make WebView invisible initially but enable it
                _videoPlayer.Opacity = 0;
                _videoPlayer.IsVisible = true;

                Debug.WriteLine($"Starting video playback for URL: {VideoUrl}");

                // Generate HTML with simpler, more reliable video playback
                string videoHtml = GetSimplifiedVideoHtml(VideoUrl);

                // Set the source - this is a fresh load each time to avoid caching issues
                _videoPlayer.Source = new HtmlWebViewSource { Html = videoHtml };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error auto-playing video: {ex.Message}");
                _loadingIndicator.IsVisible = false;
                _loadingIndicator.IsRunning = false;
                _playButtonOverlay.IsVisible = true;
            }
        }

        private string GetSimplifiedVideoHtml(string videoUrl)
        {
            // Clean the URL
            if (!videoUrl.StartsWith("http://") && !videoUrl.StartsWith("https://"))
            {
                videoUrl = "https://" + videoUrl.TrimStart('/');
            }

            // Add cache busting
            string timestamp = DateTime.Now.Ticks.ToString();
            string finalUrl = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={timestamp}"
                : $"{videoUrl}?cb={timestamp}";

            Debug.WriteLine($"Video URL with cache busting: {finalUrl}");

            // Create a simpler HTML with fewer event listeners to avoid callback issues
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body, html {{
            margin: 0;
            padding: 0;
            width: 100%;
            height: 100%;
            background-color: transparent;
            overflow: hidden;
        }}
        #videoPlayer {{
            width: 100%;
            height: 100%;
            object-fit: cover;
            background-color: transparent;
        }}
    </style>
</head>
<body>
    <video id='videoPlayer' playsinline autoplay loop {(IsMuted ? "muted" : "")}>
        <source src='{finalUrl}' type='video/mp4'>
    </video>
</body>
</html>";
        }


        public void HandleVideoPlaying()
        {
            MainThread.BeginInvokeOnMainThread(async () => {
                // Hide loading indicator and thumbnail
                _loadingIndicator.IsVisible = false;
                _loadingIndicator.IsRunning = false;
                _playButtonOverlay.IsVisible = false;

                // Fade in the video, which will hide the thumbnail beneath it
                await _videoPlayer.FadeTo(1.0, 300);

                // Once the fade is complete, we can safely hide the thumbnail to save resources
                _thumbnailImage.IsVisible = false;
            });
        }

        // Toggle mute state
        public void ToggleMute()
        {
            Debug.WriteLine($"Toggling mute from {IsMuted} to {!IsMuted}");
            IsMuted = !IsMuted;

            // Apply the change immediately if video is loaded
            if (_isVideoLoaded)
            {
                MainThread.BeginInvokeOnMainThread(async () => {
                    await ApplyMuteState();
                });
            }

            // Notify about mute state change
            MuteStateChanged?.Invoke(this, IsMuted);
        }

        // Pause the video when scrolled out of view
        public void Pause()
        {
            if (!_isVideoLoaded) return;

            try
            {
                // Pause the video using JavaScript
                if (_videoPlayer.Handler != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        try
                        {
                            // Use JavaScript to pause the video
                            await _videoPlayer.EvaluateJavaScriptAsync(
                                "var video = document.getElementById('videoPlayer'); " +
                                "if(video) { video.pause(); }");
                        }
                        catch (Exception jsEx)
                        {
                            Debug.WriteLine($"JavaScript evaluation error during pause: {jsEx.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pausing video: {ex.Message}");
            }
        }

        // HTML template for silent auto-playing videos in WebView
        private string GetVideoHtml(string videoUrl)
        {
            // Add cache busting parameter
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

            // Log the HTML generation
            Debug.WriteLine($"Generating HTML for video: {urlWithCacheBusting}, IsMuted: {IsMuted}");

            return @"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {
            margin: 0;
            padding: 0;
            background-color: transparent;
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
            object-fit: cover;
        }
    </style>
</head>
<body>
    <div class='video-container'>
        <video id='videoPlayer' playsinline loop " + (IsMuted ? "muted" : "") + @" autoplay>
            <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
            Your browser does not support HTML5 video.
        </video>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            console.log('Video player initialized');
            var video = document.getElementById('videoPlayer');
            
            // Set initial mute state and log it for debugging
            video.muted = " + (IsMuted ? "true" : "false") + @";
            console.log('Initial muted state set to: ' + video.muted);
            
            // Add event listeners for debugging
            video.addEventListener('volumechange', function() {
                console.log('Volume changed. Muted: ' + video.muted + ', Volume: ' + video.volume);
            });
            
            video.addEventListener('loadstart', function() {
                console.log('Video load started');
            });
            
            video.addEventListener('loadeddata', function() {
                console.log('Video data loaded');
                window.location.href = 'maui-callback://videoLoaded';
            });
            
            video.addEventListener('canplay', function() {
                console.log('Video can play');
                window.location.href = 'maui-callback://videoCanPlay';
            });
            
            video.addEventListener('error', function(e) {
                console.log('Video error: ' + (e.target.error ? e.target.error.code : 'unknown'));
                window.location.href = 'maui-callback://videoError';
            });
            
            video.addEventListener('playing', function() {
                console.log('Video playing');
                window.location.href = 'maui-callback://videoPlaying';
            });
            
            video.addEventListener('waiting', function() {
                console.log('Video waiting/buffering');
                window.location.href = 'maui-callback://videoWaiting';
            });
            
            // Force load
            video.load();
            
            // Try autoplay
            var playPromise = video.play();
            if (playPromise !== undefined) {
                playPromise.catch(function(error) {
                    console.log('Autoplay prevented:', error);
                });
            }
        });
        
        // Helper function to toggle mute (can be called from MAUI)
        function toggleMute() {
            var video = document.getElementById('videoPlayer');
            if(video) {
                video.muted = !video.muted;
                console.log('Muted toggled to: ' + video.muted);
                return video.muted;
            }
            return null;
        }
    </script>
</body>
</html>";
        }

        // Clean up resources
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler == null)
            {
                // Clean up resources when this control is removed
                _videoPlayer.Navigated -= VideoPlayer_Navigated;

                // Stop any playback
                Pause();
            }
        }
    }
}