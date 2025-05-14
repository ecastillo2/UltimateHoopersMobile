using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPlayerPage : ContentPage
    {
        private readonly Post _post;
        private readonly HttpClient _httpClient = new HttpClient();

        // Default constructor for design time and XAML previews
        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        // Constructor with post parameter
        public VideoPlayerPage(Post post)
        {
            InitializeComponent();
            _post = post ?? throw new ArgumentNullException(nameof(post));

            try
            {
                // Set caption if available
                if (!string.IsNullOrWhiteSpace(_post.Caption))
                {
                    captionLabel.Text = _post.Caption;
                }
                else
                {
                    captionLabel.Text = $"Video by ";
                }

                // Set thumbnail image
                string thumbnailUrl = null;
                if (!string.IsNullOrWhiteSpace(_post.ThumbnailUrl))
                {
                    thumbnailUrl = _post.ThumbnailUrl;
                    Console.WriteLine($"Using thumbnail URL: {thumbnailUrl}");
                }
                else if (!string.IsNullOrWhiteSpace(_post.PostFileURL))
                {
                    // If no thumbnail, try to use the post image as fallback
                    thumbnailUrl = _post.PostFileURL;
                    Console.WriteLine($"Using PostFileURL as thumbnail: {thumbnailUrl}");
                }

                if (!string.IsNullOrWhiteSpace(thumbnailUrl))
                {
                    // Test the URL
                    TestUrlAccessAsync(thumbnailUrl);
                    
                    // Set the thumbnail source directly
                    thumbnailImage.Source = ImageSource.FromUri(new Uri(thumbnailUrl));
                }
                else
                {
                    Console.WriteLine("No thumbnail URL available");
                }

                // Initially show the fallback grid with thumbnail and play button
                fallbackGrid.IsVisible = true;
                videoWebView.IsVisible = false;

                // Debug output
                if (_post != null)
                {
                    Console.WriteLine($"VideoPlayerPage created for post: {_post.PostId}");
                    Console.WriteLine($"PostFileURL: {_post.PostFileURL}");
                    Console.WriteLine($"ThumbnailUrl: {_post.ThumbnailUrl}");
                    Console.WriteLine($"PostType: {_post.PostType}");
                }
                else
                {
                    Console.WriteLine("VideoPlayerPage created with null post");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VideoPlayerPage constructor: {ex.Message}");
                DisplayAlert("Error", $"Error loading video: {ex.Message}", "OK");
            }
        }

        // Helper method to test URL access
        private async void TestUrlAccessAsync(string url)
        {
            try
            {
                Console.WriteLine($"Testing URL access in VideoPlayerPage: {url}");
                
                // Use HttpClient to test the URL
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    var response = await _httpClient.SendAsync(request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Successfully accessed URL: {url}. Status: {response.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to access URL: {url}. Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing URL: {url}. Error: {ex.Message}");
            }
        }

        // Handle the play button tap event
        private async void OnPlayButtonTapped(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("Play button tapped");
                
                // Only proceed if we have a valid video URL
                if (string.IsNullOrWhiteSpace(_post.PostFileURL))
                {
                    Console.WriteLine("PostFileURL is empty");
                    await DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                string videoUrl = _post.PostFileURL;
                Console.WriteLine($"Playing video URL: {videoUrl}");

                // Test the URL
                TestUrlAccessAsync(videoUrl);

                // Try to play the video directly using a direct link approach
                try
                {
                    // Create an action sheet to let the user choose how to view the video
                    string action = await DisplayActionSheet(
                        "View Video", 
                        "Cancel", 
                        null, 
                        "Play in WebView", 
                        "Play in Browser");

                    switch (action)
                    {
                        case "Play in WebView":
                            // Create an HTML wrapper for the video that will make it responsive and centered
                            string videoHtml = CreateVideoHtml(videoUrl);
                            
                            // Log the HTML for debugging
                            Console.WriteLine($"Video HTML: {videoHtml.Substring(0, Math.Min(200, videoHtml.Length))}...");
                            
                            // Load the HTML into the WebView
                            videoWebView.Source = new HtmlWebViewSource
                            {
                                Html = videoHtml
                            };

                            // Hide the fallback and show the WebView
                            fallbackGrid.IsVisible = false;
                            videoWebView.IsVisible = true;
                            break;
                            
                        case "Play in Browser":
                            await Launcher.OpenAsync(new Uri(videoUrl));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing video: {ex.Message}");
                    await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
                    
                    // Show the play button again if there was an error
                    playButtonFrame.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnPlayButtonTapped: {ex.Message}");
                await DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");
                
                // Show the play button again if there was an error
                playButtonFrame.IsVisible = true;
            }
        }

        private string CreateVideoHtml(string videoUrl)
        {
            // Create an HTML document with a video element
            // Add cache-busting query parameter to prevent caching issues
            string urlWithCacheBusting = videoUrl + "?t=" + DateTime.Now.Ticks;
            
            return @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
                <style>
                    body {
                        margin: 0;
                        padding: 0;
                        background-color: #000;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        height: 100vh;
                        overflow: hidden;
                    }
                    .video-container {
                        width: 100%;
                        max-width: 100%;
                        height: 100%;
                        max-height: 100%;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                    }
                    video {
                        max-width: 100%;
                        max-height: 100%;
                        width: auto;
                        height: auto;
                    }
                    .error-message {
                        color: white;
                        text-align: center;
                        padding: 20px;
                        display: none;
                    }
                    .open-in-browser {
                        background-color: #512BD4;
                        color: white;
                        border: none;
                        padding: 10px 20px;
                        border-radius: 5px;
                        margin-top: 20px;
                        cursor: pointer;
                    }
                </style>
            </head>
            <body>
                <div class='video-container'>
                    <video id='video-player' controls autoplay>
                        <source src='" + urlWithCacheBusting + @"' type='video/mp4'>
                        Your browser does not support the video tag.
                    </video>
                    <div id='error-message' class='error-message'>
                        <p>Unable to play the video in the app.</p>
                        <p>This may be due to security restrictions.</p>
                    </div>
                </div>
                <script>
                    // Log when video starts loading
                    document.addEventListener('DOMContentLoaded', function() {
                        var video = document.getElementById('video-player');
                        var errorMessage = document.getElementById('error-message');
                        
                        console.log('Video element created for: " + urlWithCacheBusting + @"');
                        
                        video.addEventListener('loadstart', function() {
                            console.log('Video load started');
                        });
                        
                        video.addEventListener('loadeddata', function() {
                            console.log('Video data loaded');
                        });
                        
                        video.addEventListener('error', function(e) {
                            console.log('Video error: ' + (video.error ? video.error.code : 'unknown'));
                            video.style.display = 'none';
                            errorMessage.style.display = 'block';
                        });
                        
                        // Force reload
                        video.load();
                    });
                </script>
            </body>
            </html>";
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // Close the video page
            Navigation.PopModalAsync();
        }
    }
}