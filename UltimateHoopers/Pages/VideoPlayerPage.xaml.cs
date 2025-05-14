using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace UltimateHoopers.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPlayerPage : ContentPage
    {
        private readonly Post _post;

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

            // Set caption if available
            if (!string.IsNullOrEmpty(_post.Caption))
            {
                captionLabel.Text = _post.Caption;
            }
            else
            {
                captionLabel.Text = $"Video by {_post.UserName}";
            }

            // Set thumbnail image
            if (!string.IsNullOrEmpty(_post.ThumbnailUrl))
            {
                thumbnailImage.Source = _post.ThumbnailUrl;
            }
            else if (!string.IsNullOrEmpty(_post.PostFileURL))
            {
                // If no thumbnail, try to use the post image as fallback
                thumbnailImage.Source = _post.PostFileURL;
            }

            // Initially show the fallback grid with thumbnail and play button
            fallbackGrid.IsVisible = true;
            videoWebView.IsVisible = false;
        }

        // Add the missing OnPlayButtonTapped event handler with the correct signature
        private void OnPlayButtonTapped(object sender, EventArgs e)
        {
            try
            {
                // Only proceed if we have a valid video URL
                if (string.IsNullOrEmpty(_post.PostFileURL))
                {
                    DisplayAlert("Error", "Video URL is not available", "OK");
                    return;
                }

                // Show loading indicator
                playButtonFrame.IsVisible = false;

                // Create an HTML wrapper for the video that will make it responsive and centered
                string videoHtml = CreateVideoHtml(_post.PostFileURL);

                // Load the HTML into the WebView
                videoWebView.Source = new HtmlWebViewSource
                {
                    Html = videoHtml
                };

                // Hide the fallback and show the WebView
                fallbackGrid.IsVisible = false;
                videoWebView.IsVisible = true;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Could not play video: {ex.Message}", "OK");

                // Show the play button again if there was an error
                playButtonFrame.IsVisible = true;
            }
        }

        private string CreateVideoHtml(string videoUrl)
        {
            // Create an HTML document with a video element
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
                    video {
                        max-width: 100%;
                        max-height: 100%;
                        width: auto;
                        height: auto;
                    }
                </style>
            </head>
            <body>
                <video controls autoplay>
                    <source src='" + videoUrl + @"' type='video/mp4'>
                    Your browser does not support the video tag.
                </video>
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