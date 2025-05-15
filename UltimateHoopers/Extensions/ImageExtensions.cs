using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using UltimateHoopers.Helpers;

namespace UltimateHoopers.Extensions
{
    public static class ImageExtensions
    {
        public static async Task LoadImageSafelyAsync(this Image image, string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            try
            {
                // Check if WebP or special format
                if (imageUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    // Use cached image path for WebP or just set source directly
                    string cachedPath = await ImageCacheHelper.GetImageFilePathAsync(imageUrl);
                    if (!string.IsNullOrEmpty(cachedPath))
                    {
                        image.Source = cachedPath;
                    }
                    else
                    {
                        // Fallback to direct loading
                        image.Source = ImageSource.FromUri(new Uri(imageUrl));
                    }
                }
                else
                {
                    // Standard image formats
                    image.Source = ImageSource.FromUri(new Uri(imageUrl));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                // Use placeholder for failed images
                image.Source = "dotnet_bot.png"; // Replace with your placeholder image
            }
        }
    }
}