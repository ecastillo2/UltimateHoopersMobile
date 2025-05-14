using Domain;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UltimateHoopers.Services;

namespace UltimateHoopers.Pages
{
    public partial class PostsPage : ContentPage
    {
        private readonly ObservableCollection<Post> _posts = new ObservableCollection<Post>();
        private readonly PostService _postService;

        public PostsPage()
        {
            try
            {
                InitializeComponent();
                _postService = new PostService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in PostsPage constructor: {ex.Message}");
                // Don't rethrow here, as it might crash the app during initialization
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPostsAsync();
        }

        private async Task LoadPostsAsync()
        {
            try
            {
                // Check if we have a valid post service
                if (_postService == null)
                {
                    await DisplayAlert("Error", "Post service is not available", "OK");
                    return;
                }

                // Try to get posts
                var posts = await _postService.GetPostsAsync();

                if (posts != null && posts.Count > 0)
                {
                    _posts.Clear();
                    foreach (var post in posts)
                    {
                        _posts.Add(post);
                    }

                    // If you had a ListView or CollectionView, you would set its ItemsSource here
                    // PostsList.ItemsSource = _posts;

                    await DisplayAlert("Posts Loaded", $"Successfully loaded {posts.Count} posts", "OK");
                }
                else
                {
                    await DisplayAlert("No Posts", "No posts were found", "OK");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle specifically the unauthorized access exception (no token)
                await DisplayAlert("Authentication Error", "Please log in to view posts", "OK");

                // Optionally, navigate to login page
                // await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Rest of the event handlers...
    }
}