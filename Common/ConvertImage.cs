using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Common
{
    public static class ConvertImage
    {
        // Define image dimensions constants
        private static readonly Dictionary<string, (int width, int height)> _mediaDimensions = new Dictionary<string, (int, int)>
        {
            { "image", (640, 426) },
            { "video", (800, 535) }
        };

        /// <summary>
        /// Convert image to optimized WebP format with appropriate dimensions
        /// </summary>
        /// <param name="type">Media type: "image" or "video"</param>
        /// <param name="file">The file to convert</param>
        /// <returns>WebP encoded byte array</returns>
        public static async Task<byte[]> ConvertToWebPAsync(string type, IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!_mediaDimensions.TryGetValue(type, out var dimensions))
                throw new ArgumentException($"Invalid media type: {type}", nameof(type));

            using var inputStream = file.OpenReadStream();
            using var originalImage = SKBitmap.Decode(inputStream);

            // Resize the image to the appropriate dimensions
            using var resizedImage = originalImage.Resize(
                new SKImageInfo(dimensions.width, dimensions.height),
                SKFilterQuality.High);

            using var outputStream = new MemoryStream();
            // Encode the resized image as WebP
            resizedImage.Encode(outputStream, SKEncodedImageFormat.Webp, 75);

            return outputStream.ToArray();
        }
    }
}