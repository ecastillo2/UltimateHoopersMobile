using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimateHoopers.Pages
{
    public partial class ShopPage : ContentPage
    {
        // Mock shopping cart items count
        private int _cartItemsCount = 2;

        // Selected category
        private string _selectedCategory = "All";

        // Sample product data - in a real app, this would come from a service
        private List<Product> _products;

        public ShopPage()
        {
            InitializeComponent();

            // Initialize product data
            InitializeProducts();
        }

        private void InitializeProducts()
        {
            // Mock product data
            _products = new List<Product>
            {
                new Product
                {
                    Id = "1",
                    Name = "Pro Basketball Shoes",
                    Description = "High performance basketball shoes",
                    Price = 129.99m,
                    Category = "Shoes",
                    ImageUrl = "https://placehold.co/150x150",
                    IsNew = false
                },
                new Product
                {
                    Id = "2",
                    Name = "Premium Basketball",
                    Description = "Official size and weight",
                    Price = 49.99m,
                    Category = "Basketballs",
                    ImageUrl = "https://placehold.co/150x150",
                    IsNew = false
                },
                new Product
                {
                    Id = "3",
                    Name = "Performance Jersey",
                    Description = "Breathable basketball jersey",
                    Price = 39.99m,
                    Category = "Apparel",
                    ImageUrl = "https://placehold.co/150x150",
                    IsNew = false
                },
                new Product
                {
                    Id = "4",
                    Name = "Basketball Shorts",
                    Description = "Lightweight and durable",
                    Price = 34.99m,
                    Category = "Apparel",
                    ImageUrl = "https://placehold.co/150x150",
                    IsNew = false
                },
                new Product
                {
                    Id = "5",
                    Name = "Basketball Training Kit",
                    Description = "Complete training system",
                    Price = 89.99m,
                    Category = "Equipment",
                    ImageUrl = "https://placehold.co/150x150",
                    IsNew = true
                },
                new Product
                {
                    Id = "6",
                    Name = "Basketball Shooting Sleeve",
                    Description = "Compression technology",
                    Price = 19.99m,
                    Category = "Accessories",
                    ImageUrl = "https://placehold.co/150x150",
                    IsNew = true
                }
            };
        }

        // Navigation methods
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnHomeNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                // Check if we're already on the HomePage
                Page currentPage = null;

                if (Shell.Current != null)
                {
                    currentPage = Shell.Current.CurrentPage;
                }
                else if (Application.Current?.MainPage != null)
                {
                    currentPage = Application.Current.MainPage;
                }

                // If we're already on HomePage, do nothing
                if (currentPage is HomePage)
                {
                    Console.WriteLine("Already on HomePage, skipping navigation");
                    return;
                }

                Console.WriteLine("Navigating to HomePage");
                await Shell.Current.GoToAsync("//HomePage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to HomePage: {ex.Message}");
            }
        }

        private async void OnGamesNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                // Check if we're already on the FindRunsPage
                Page currentPage = null;

                if (Shell.Current != null)
                {
                    currentPage = Shell.Current.CurrentPage;
                }
                else if (Application.Current?.MainPage != null)
                {
                    currentPage = Application.Current.MainPage;
                }

                // If we're already on FindRunsPage, do nothing
                if (currentPage is FindRunsPage)
                {
                    Console.WriteLine("Already on FindRunsPage, skipping navigation");
                    return;
                }

                Console.WriteLine("Navigating to FindRunsPage");
                await Shell.Current.GoToAsync("//FindRunsPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to FindRunsPage: {ex.Message}");
            }
        }

        private async void OnProfileNavigationClicked(object sender, EventArgs e)
        {
            try
            {
                await DisplayAlert("Profile", "Profile page coming soon!", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing profile alert: {ex.Message}");
            }
        }

        // Shop functionality methods
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // Filter products based on search text
            FilterProducts();
        }

        private void OnCategorySelected(object sender, EventArgs e)
        {
            if (sender is not Frame categoryFrame)
                return;

            // Get the label inside the frame
            if (categoryFrame.Content is not Label categoryLabel)
                return;

            // Update selected category
            _selectedCategory = categoryLabel.Text;

            // Update UI to show selected category
            UpdateCategorySelection(categoryFrame);

            // Filter products based on selected category
            FilterProducts();
        }

        private void UpdateCategorySelection(Frame selectedFrame)
        {
            // Reset all category frames to unselected style
            foreach (var child in ((HorizontalStackLayout)((ScrollView)((VerticalStackLayout)((Grid)Content).Children[1]).Children[0]).Content).Children)
            {
                if (child is Frame frame)
                {
                    // Reset to unselected style
                    frame.BackgroundColor = (Color)Application.Current.Resources["CardBackgroundColor"];
                    frame.BorderColor = (Color)Application.Current.Resources["BorderColor"];

                    if (frame.Content is Label label)
                    {
                        label.TextColor = (Color)Application.Current.Resources["PrimaryTextColor"];
                        label.FontAttributes = FontAttributes.None;
                    }
                }
            }

            // Set selected frame to selected style
            selectedFrame.BackgroundColor = (Color)Application.Current.Resources["PrimaryColor"];
            selectedFrame.BorderColor = (Color)Application.Current.Resources["PrimaryColor"];

            if (selectedFrame.Content is Label selectedLabel)
            {
                selectedLabel.TextColor = Colors.White;
                selectedLabel.FontAttributes = FontAttributes.Bold;
            }
        }

        private void FilterProducts()
        {
            string searchText = SearchEntry.Text?.ToLower() ?? string.Empty;

            // Apply both category and search filters
            var filteredProducts = _products.Where(p =>
                (_selectedCategory == "All" || p.Category == _selectedCategory) &&
                (string.IsNullOrEmpty(searchText) ||
                 p.Name.ToLower().Contains(searchText) ||
                 p.Description.ToLower().Contains(searchText)))
                .ToList();

            // In a real app, you would update the products collection with the filtered results
            Console.WriteLine($"Filtered products: {filteredProducts.Count} items");
            Console.WriteLine($"Filter criteria: Category='{_selectedCategory}', Search='{searchText}'");

            // Update UI with filtered products
            // This would typically update a CollectionView or ListView
        }

        private async void OnAddToCartClicked(object sender, EventArgs e)
        {
            // Get the product from the button's context
            string productName = "Product";

            if (sender is Button button)
            {
                // Try to determine which product was clicked based on UI hierarchy
                var parent = button.Parent;
                while (parent != null)
                {
                    if (parent is Grid grid && grid.Children.Count > 1)
                    {
                        // Find the product name label in the grid children
                        foreach (var child in grid.Children)
                        {
                            if (child is VerticalStackLayout stack)
                            {
                                foreach (var stackChild in stack.Children)
                                {
                                    if (stackChild is Label label && label.FontAttributes.HasFlag(FontAttributes.Bold))
                                    {
                                        productName = label.Text;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    parent = parent.Parent;
                }
            }

            // Increment cart count
            _cartItemsCount++;

            // Show confirmation
            await DisplayAlert("Added to Cart", $"{productName} has been added to your cart.", "OK");

            // In a real app, you would update the cart badge UI here
        }

        private async void OnCartClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Shopping Cart", "Shopping cart feature coming soon!", "OK");
        }

        private async void OnLoadMoreClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Load More", "Loading more products...", "OK");
            // In a real app, you would load the next page of products
        }
    }

    // Simple product model class
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public bool IsNew { get; set; }
    }
}