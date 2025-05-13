using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services
{
    /// <summary>
    /// Service for processing and validating file uploads
    /// </summary>
    public class FileProcessorService : IFileProcessorService
    {
        private readonly ILogger<FileProcessorService> _logger;

        // Use readonly collections that are initialized once
        private static readonly Dictionary<string, string> _mediaContentTypes = new Dictionary<string, string>
        {
            // Images
            { "image/jpeg", "image" },
            { "image/png", "image" },
            { "image/gif", "image" },
            { "image/webp", "image" },
            
            // Videos
            { "video/mp4", "video" },
            { "video/x-msvideo", "video" },
            { "video/x-flv", "video" },
            { "video/x-matroska", "video" },
            { "video/quicktime", "video" }
        };

        // Define image dimensions constants
        private static readonly Dictionary<string, (int width, int height)> _mediaDimensions = new Dictionary<string, (int, int)>
        {
            { "image", (640, 426) },
            { "video", (800, 535) }
        };

        public FileProcessorService(ILogger<FileProcessorService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Determines the type of file based on its MIME content type
        /// </summary>
        /// <param name="file">The uploaded file to check</param>
        /// <returns>The file type category or an error message</returns>
        public string CheckFileType(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Empty file uploaded");
                return "No file uploaded.";
            }

            var isValidType = _mediaContentTypes.TryGetValue(file.ContentType, out string fileType);

            if (!isValidType)
            {
                _logger.LogWarning($"Invalid file type: {file.ContentType}");
            }

            return isValidType
                ? fileType
                : "Invalid file type. Only images and videos are allowed.";
        }

        /// <summary>
        /// Convert image to optimized WebP format with appropriate dimensions
        /// </summary>
        /// <param name="type">Media type: "image" or "video"</param>
        /// <param name="file">The file to convert</param>
        /// <returns>WebP encoded byte array</returns>
        public async Task<byte[]> ConvertToWebPAsync(string type, IFormFile file)
        {
            if (file == null)
            {
                _logger.LogError("Null file provided for conversion");
                throw new ArgumentNullException(nameof(file));
            }

            if (!_mediaDimensions.TryGetValue(type, out var dimensions))
            {
                _logger.LogError($"Invalid media type: {type}");
                throw new ArgumentException($"Invalid media type: {type}", nameof(type));
            }

            try
            {
                using var inputStream = file.OpenReadStream();
                using var originalImage = SkiaSharp.SKBitmap.Decode(inputStream);

                // Resize the image to the appropriate dimensions
                using var resizedImage = originalImage.Resize(
                    new SkiaSharp.SKImageInfo(dimensions.width, dimensions.height),
                    SkiaSharp.SKFilterQuality.High);

                using var outputStream = new MemoryStream();
                // Encode the resized image as WebP
                resizedImage.Encode(outputStream, SkiaSharp.SKEncodedImageFormat.Webp, 75);

                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error converting {type} to WebP format");
                throw new ApplicationException($"Error converting {type} to WebP format", ex);
            }
        }
    }

    public interface IFileProcessorService
    {
        string CheckFileType(IFormFile file);
        Task<byte[]> ConvertToWebPAsync(string type, IFormFile file);
    }
}