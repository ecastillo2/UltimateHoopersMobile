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
using UltimateHoopers.Helpers; // Add this to import NavigationHelper

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

        // Override this method to help with audio playback on appearing
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Your existing code here...
        }

        // Navigation methods - Updated to use NavigationHelper

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await NavigationHelper.NavigateTo(this, "//HomePage");
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

        // All other methods in your original file...
    }
}