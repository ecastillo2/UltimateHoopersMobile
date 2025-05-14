using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UltimateHoopers.Helpers
{
    public static class ImageCacheHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, string> _memoryCache = new Dictionary<string, string>();
        private static readonly string _cacheFolder = Path.Combine(FileSystem.CacheDirectory, "ImageCache");

        static ImageCacheHelper()
        {
            // Ensure cache directory exists
            if (!Directory.Exists(_cacheFolder))
            {
                Directory.CreateDirectory(_cacheFolder);
            }
        }

        /// <summary>
        /// Gets an image from the cache or downloads it if not available
        /// </summary>
        public static async Task<string> GetImageFilePathAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            // Check if URL is already a local file path
            if (imageUrl.StartsWith("file://") || Path.IsPathRooted(imageUrl))
                return imageUrl;

            // Generate a unique file name based on the URL
            string fileName = GetFileNameFromUrl(imageUrl);
            string filePath = Path.Combine(_cacheFolder, fileName);

            // Check memory cache first
            if (_memoryCache.TryGetValue(imageUrl, out string cachedPath))
            {
                if (File.Exists(cachedPath))
                    return cachedPath;
                else
                    _memoryCache.Remove(imageUrl);
            }

            // Check if file exists in local cache
            if (File.Exists(filePath))
            {
                _memoryCache[imageUrl] = filePath;
                return filePath;
            }

            // Download the image
            try
            {
                byte[] imageData = await _httpClient.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(filePath, imageData);
                _memoryCache[imageUrl] = filePath;
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error caching image: {ex.Message}");
                return imageUrl; // Fall back to original URL
            }
        }

        /// <summary>
        /// Clears the image cache
        /// </summary>
        public static void ClearCache()
        {
            _memoryCache.Clear();
            try
            {
                if (Directory.Exists(_cacheFolder))
                {
                    foreach (var file in Directory.GetFiles(_cacheFolder))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate a unique file name from a URL
        /// </summary>
        private static string GetFileNameFromUrl(string url)
        {
            // Extract extension if present
            string extension = ".dat";

            if (url.Contains("?"))
                url = url.Substring(0, url.IndexOf('?'));

            if (url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                extension = ".webp";
            else if (url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                     url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                extension = ".jpg";
            else if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                extension = ".png";
            else if (url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                extension = ".gif";
            else if (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                extension = ".mp4";

            // Create a hash of the URL for the filename
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(url));
                string filename = BitConverter.ToString(hash).Replace("-", string.Empty);
                return filename + extension;
            }
        }
    }
}