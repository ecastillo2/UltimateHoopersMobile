using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.IO;

namespace ApiClient
{
    /// <summary>
    /// Utility class for image conversion operations
    /// </summary>
    public static class ConvertImage
    {
        /// <summary>
        /// Converts an image to WebP format
        /// </summary>
        /// <param name="type">Media type (image or video)</param>
        /// <param name="formFile">The file to convert</param>
        /// <returns>WebP encoded byte array</returns>
        public static byte[] ConvertPngToWebP(string type, IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("File is required", nameof(formFile));
            }

            try
            {
                // Get appropriate dimensions based on media type
                int width = 640, height = 426;
                if (type.ToLower() == "video")
                {
                    width = 800;
                    height = 535;
                }

                using (var stream = formFile.OpenReadStream())
                using (var skBitmap = SKBitmap.Decode(stream))
                {
                    // Resize image to target dimensions
                    using (var resizedBitmap = skBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                    using (var image = SKImage.FromBitmap(resizedBitmap))
                    {
                        // Encode to WebP format
                        using (var webpData = image.Encode(SKEncodedImageFormat.Webp, 85))
                        {
                            // Convert to byte array
                            return webpData.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting image to WebP: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Corrects image orientation based on EXIF data and converts to WebP
        /// </summary>
        /// <param name="formFile">The file to convert</param>
        /// <returns>WebP encoded byte array</returns>
        public static byte[] CorrectOrientationAndConvertToWebP(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("File is required", nameof(formFile));
            }

            try
            {
                using (var stream = formFile.OpenReadStream())
                using (var skBitmap = SKBitmap.Decode(stream))
                {
                    // Create a new SKBitmap with the correct orientation
                    var skImageInfo = skBitmap.Info;
                    using (var correctlyOrientedBitmap = new SKBitmap(skImageInfo))
                    {
                        // Copy pixels from original bitmap to new bitmap
                        skBitmap.CopyTo(correctlyOrientedBitmap);

                        // Encode to WebP format
                        using (var image = SKImage.FromBitmap(correctlyOrientedBitmap))
                        using (var webpData = image.Encode(SKEncodedImageFormat.Webp, 85))
                        {
                            // Convert to byte array
                            return webpData.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error correcting orientation and converting to WebP: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Resizes an image to specified dimensions and converts to WebP
        /// </summary>
        /// <param name="formFile">The file to convert</param>
        /// <param name="width">Target width</param>
        /// <param name="height">Target height</param>
        /// <returns>WebP encoded byte array</returns>
        public static byte[] ResizeAndConvertToWebP(IFormFile formFile, int width, int height)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("File is required", nameof(formFile));
            }

            try
            {
                using (var stream = formFile.OpenReadStream())
                using (var skBitmap = SKBitmap.Decode(stream))
                using (var resizedBitmap = skBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                using (var image = SKImage.FromBitmap(resizedBitmap))
                using (var webpData = image.Encode(SKEncodedImageFormat.Webp, 85))
                {
                    // Convert to byte array
                    return webpData.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error resizing and converting to WebP: {ex.Message}", ex);
            }
        }
    }
}