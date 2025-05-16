using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UltimateHoopers.Controls
{
    /// <summary>
    /// A custom control for auto-playing videos during scrolling with no sound
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

        // Get/set post object
        public Domain.Post Post
        {
            get => (Domain.Post)GetValue(PostProperty);
            set => SetValue(PostProperty, value);
        }

        // Event to notify when tapping for full-screen playback
        public event EventHandler<EventArgs> FullScreenRequested;

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
        private void VideoPlayer_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Success)
            {
                _isVideoLoaded = true;
                _loadingIndicator.IsVisible = false;
                _loadingIndicator.IsRunning = false;

                // Fade in the video player
                MainThread.BeginInvokeOnMainThread(async () => {
                    await _videoPlayer.FadeTo(1.0, 300);
                    _playButtonOverlay.IsVisible = false;
                });
            }
            else
            {
                Debug.WriteLine($"Video navigation failed: {e.Result}");
                _isVideoLoaded = false;
                _loadingIndicator.IsVisible = false;
                _loadingIndicator.IsRunning = false;
                _playButtonOverlay.IsVisible = true;
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

                // Generate the HTML with silent auto-playing video
                string videoHtml = GetVideoHtml(VideoUrl);

                // Load video source if not already loaded
                if (!_isVideoLoaded)
                {
                    _videoPlayer.Source = new HtmlWebViewSource { Html = videoHtml };
                }
                else
                {
                    // Try to resume without reloading
                    try
                    {
                        // Try to resume using JavaScript
                        // Note: Eval may not be available in all MAUI WebView implementations
                        if (_videoPlayer.Handler != null)
                        {
                            // Try running JavaScript directly in the WebView
                            MainThread.BeginInvokeOnMainThread(async () => {
                                try
                                {
                                    // Use URL navigation for callbacks as a more reliable mechanism
                                    await _videoPlayer.EvaluateJavaScriptAsync(
                                        "var video = document.getElementById('videoPlayer'); " +
                                        "if(video) { " +
                                        "  video.play().then(() => { " +
                                        "    window.location.href = 'maui-callback://videoPlaying'; " +
                                        "  }).catch(err => { " +
                                        "    console.log('Play error: ' + err); " +
                                        "  }); " +
                                        "}");
                                }
                                catch (Exception jsEx)
                                {
                                    Debug.WriteLine($"JavaScript evaluation error: {jsEx.Message}");
                                    // Fall back to reloading the video
                                    _videoPlayer.Source = new HtmlWebViewSource { Html = videoHtml };
                                }
                            });
                        }
                        else
                        {
                            // If Handler is null, reload the video
                            _videoPlayer.Source = new HtmlWebViewSource { Html = videoHtml };
                        }
                    }
                    catch
                    {
                        // If JavaScript evaluation fails, reload the video
                        _videoPlayer.Source = new HtmlWebViewSource { Html = videoHtml };
                    }
                }

                _videoPlayer.IsVisible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error auto-playing video: {ex.Message}");
                _loadingIndicator.IsVisible = false;
                _loadingIndicator.IsRunning = false;
                _playButtonOverlay.IsVisible = true;
            }
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
            // Add cache busting to avoid caching issues
            string cacheBustParam = DateTime.Now.Ticks.ToString();
            string urlWithCacheBusting = videoUrl.Contains("?")
                ? $"{videoUrl}&cb={cacheBustParam}"
                : $"{videoUrl}?cb={cacheBustParam}";

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
            object-fit: cover;
        }
    </style>
</head>
<body>
    <div class='video-container'>
        <video id='videoPlayer' playsinline loop muted autoplay>
            <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
            Your browser does not support HTML5 video.
        </video>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            var video = document.getElementById('videoPlayer');
            
            // Make sure video is muted
            video.muted = true;
            
            video.addEventListener('canplay', function() {
                window.location.href = 'maui-callback://videoCanPlay';
            });
            
            video.addEventListener('error', function(e) {
                window.location.href = 'maui-callback://videoError';
            });
            
            video.addEventListener('playing', function() {
                window.location.href = 'maui-callback://videoPlaying';
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
            
            // Handle click to unmute/mute
            video.addEventListener('click', function() {
                // In the feed view we don't want to enable sound on tap
                // That would be handled in the full-screen view
                video.muted = true;
            });
        });
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