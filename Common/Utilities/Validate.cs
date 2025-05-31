using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public static class Validate
    {
        /// <summary>
        /// Validate image file
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return (false, "No image file provided");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, "Invalid file type. Only image files are allowed (JPG, PNG, GIF, BMP, WEBP).");
            }

            // Validate file size (5MB limit)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (imageFile.Length > maxFileSize)
            {
                return (false, "File size exceeds 5MB limit.");
            }

            // Validate content type
            var allowedContentTypes = new[] {
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "image/bmp", "image/webp"
            };

            if (!allowedContentTypes.Contains(imageFile.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid content type. Only image files are allowed.");
            }

            return (true, null);
        }

        /// <summary>
        /// Validate image URL
        /// </summary>
        public static async Task<bool> ValidateImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                return false;

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                using var response = await httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, uri));

                if (!response.IsSuccessStatusCode)
                    return false;

                var contentType = response.Content.Headers.ContentType?.MediaType;
                return !string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/");
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "Error validating image URL: {ImageUrl}", imageUrl);
                return false;
            }
        }
    }
}
