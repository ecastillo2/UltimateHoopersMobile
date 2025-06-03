using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.ApiClients
{
    public class StorageApi : IStorageApi
    {
        private readonly ILogger<StorageApi> _logger;
        private readonly string _connectionString;
        private readonly string _productImageContainerName;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly HttpClient _httpClient;
        private const int MAX_IMAGE_WIDTH = 1200;
        private const int MAX_IMAGE_HEIGHT = 1200;
        private const int PRODUCT_IMAGE_WIDTH = 800;
        private const int PRODUCT_IMAGE_HEIGHT = 409;
        private const int JPEG_QUALITY = 85;
        private const int WEBP_QUALITY = 90;

        /// <summary>
        /// Storage API Constructor
        /// </summary>
        /// <param name="httpClient">HTTP client instance</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        public StorageApi(HttpClient httpClient, IConfiguration configuration, ILogger<StorageApi> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _connectionString = configuration["BlobStorage:ConnectionString"] ?? ""
                ?? throw new InvalidOperationException("Blob storage connection string is not configured");

            _productImageContainerName = configuration["BlobStorage:ProductContainerName"] ?? ""
                ?? throw new InvalidOperationException("Profile container name is not configured");

            _blobServiceClient = new BlobServiceClient(_connectionString);
        }

        /// <summary>
        /// Removes Exif data from an image to prevent privacy issues
        /// </summary>
        /// <param name="imageStream">Source image stream</param>
        /// <returns>Clean image stream without Exif data</returns>
        public async Task<MemoryStream> RemoveExifDataAsync(Stream imageStream)
        {
            try
            {
                imageStream.Position = 0;

                using var image = Image.FromStream(imageStream);
                var outputStream = new MemoryStream();

                // Create a new bitmap without metadata
                using (var cleanBitmap = new Bitmap(image.Width, image.Height))
                {
                    using (var graphics = Graphics.FromImage(cleanBitmap))
                    {
                        graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                    }

                    // Save without metadata
                    cleanBitmap.Save(outputStream, ImageFormat.Jpeg);
                }

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Exif data from image");

                // Fallback: return original stream
                imageStream.Position = 0;
                var fallbackStream = new MemoryStream();
                await imageStream.CopyToAsync(fallbackStream);
                fallbackStream.Position = 0;
                return fallbackStream;
            }
        }

        /// <summary>
        /// Process image before upload (resize, compress, remove Exif data)
        /// </summary>
        /// <param name="file">Image file from form upload</param>
        /// <param name="maxWidth">Maximum width (optional)</param>
        /// <param name="maxHeight">Maximum height (optional)</param>
        /// <returns>Processed image as a memory stream</returns>
        public async Task<MemoryStream> ProcessImageForUploadAsync(IFormFile file, int? maxWidth = null, int? maxHeight = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was provided", nameof(file));
            }

            if (!IsValidImageFile(file))
            {
                throw new ArgumentException("Invalid image file format", nameof(file));
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var cleanStream = await RemoveExifDataAsync(stream);
                using var image = Image.FromStream(cleanStream);

                // Correct orientation first
                CorrectImageOrientation(image);

                // Calculate resize dimensions
                var targetWidth = maxWidth ?? MAX_IMAGE_WIDTH;
                var targetHeight = maxHeight ?? MAX_IMAGE_HEIGHT;
                var (newWidth, newHeight) = CalculateResizeDimensions(image.Width, image.Height, targetWidth, targetHeight);

                // Create resized image
                using var resizedImage = ResizeImage(image, newWidth, newHeight);

                // Compress and return
                return CompressImage(resizedImage, JPEG_QUALITY);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image for upload");
                throw;
            }
        }

        /// <summary>
        /// Upload product image file to blob storage
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="type">File type (image, video, etc.)</param>
        /// <param name="formFile">Uploaded file</param>
        /// <param name="timeStamp">Optional timestamp</param>
        /// <returns>Success status</returns>
        public async Task<bool> UpdateProductImageFileAsync(string productId, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(productId))
                {
                    _logger?.LogError("ProductId is null or empty");
                    return false;
                }

                if (formFile == null || formFile.Length == 0)
                {
                    _logger?.LogError("FormFile is null or empty");
                    return false;
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp" };
                if (!allowedTypes.Contains(formFile.ContentType.ToLower()))
                {
                    _logger?.LogError($"Unsupported file type: {formFile.ContentType}");
                    return false;
                }

                // Validate file size (e.g., max 10MB)
                if (formFile.Length > 10 * 1024 * 1024)
                {
                    _logger?.LogError($"File size too large: {formFile.Length} bytes");
                    return false;
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(_productImageContainerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = $"{productId}.webp";
                var blobClient = containerClient.GetBlobClient(fileName);

                // Process image and upload
                using var processedImageStream = await ProcessImageForWebP(formFile);

                if (processedImageStream == null || processedImageStream.Length == 0)
                {
                    _logger?.LogError("Processed image stream is null or empty");
                    return false;
                }

                // Reset stream position to beginning
                processedImageStream.Position = 0;

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/webp",
                        CacheControl = "public, max-age=31536000" // 1 year cache
                    },
                    Conditions = null, // Allow overwrite
                    ProgressHandler = null
                };

                await blobClient.UploadAsync(processedImageStream, uploadOptions);

                _logger?.LogInformation($"Successfully uploaded image for product {productId}");
                return true;
            }
            catch (ArgumentException ex)
            {
                _logger?.LogError(ex, $"Invalid parameter while processing image for product {productId}: {ex.Message}");
                return false;
            }
            catch (NotSupportedException ex)
            {
                _logger?.LogError(ex, $"Image format not supported for product {productId}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error uploading image for product {productId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Upload product image file to blob storage
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="type">File type (image, video, etc.)</param>
        /// <param name="formFile">Uploaded file</param>
        /// <param name="timeStamp">Optional timestamp</param>
        /// <returns>Success status</returns>
        public async Task<bool> UpdateVideoFileAsync(string videoId, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(videoId))
                {
                    _logger?.LogError("videoId is null or empty");
                    return false;
                }

                if (formFile == null || formFile.Length == 0)
                {
                    _logger?.LogError("FormFile is null or empty");
                    return false;
                }

                // Validate file type
                var allowedTypes = new[] { "video/mp4" };
                if (!allowedTypes.Contains(formFile.ContentType.ToLower()))
                {
                    _logger?.LogError($"Unsupported file type: {formFile.ContentType}");
                    return false;
                }

                // Validate file size (e.g., max 10MB)
                if (formFile.Length > 10 * 1024 * 1024)
                {
                    _logger?.LogError($"File size too large: {formFile.Length} bytes");
                    return false;
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(_productImageContainerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = $"{videoId}.mp4";
                var blobClient = containerClient.GetBlobClient(fileName);

                // Process image and upload
                using var processedImageStream = await ProcessImageForWebP(formFile);

                if (processedImageStream == null || processedImageStream.Length == 0)
                {
                    _logger?.LogError("Processed image stream is null or empty");
                    return false;
                }

                // Reset stream position to beginning
                processedImageStream.Position = 0;

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/webp",
                        CacheControl = "public, max-age=31536000" // 1 year cache
                    },
                    Conditions = null, // Allow overwrite
                    ProgressHandler = null
                };

                await blobClient.UploadAsync(processedImageStream, uploadOptions);

                _logger?.LogInformation($"Successfully uploaded image for product {videoId}");
                return true;
            }
            catch (ArgumentException ex)
            {
                _logger?.LogError(ex, $"Invalid parameter while processing image for product {videoId}: {ex.Message}");
                return false;
            }
            catch (NotSupportedException ex)
            {
                _logger?.LogError(ex, $"Image format not supported for product {videoId}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error uploading image for product {videoId}: {ex.Message}");
                return false;
            }
        }

        private async Task<MemoryStream> ProcessImageForWebP(IFormFile formFile)
        {
            try
            {
                using var inputStream = formFile.OpenReadStream();

                // Read the image data into a byte array first
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Validate that we can read the image
                if (imageBytes.Length == 0)
                {
                    throw new ArgumentException("Image file is empty");
                }

                // Process with ImageSharp (recommended) or System.Drawing
                return await ProcessImageWithImageSharp(imageBytes);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error processing image: {ex.Message}");
                throw new ArgumentException($"Failed to process image: {ex.Message}", ex);
            }
        }

        private async Task<MemoryStream> ProcessImageWithImageSharp(byte[] imageBytes)
        {
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(imageBytes);

                // Resize if needed (optional)
                var maxWidth = 1200;
                var maxHeight = 1200;

                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    var ratioX = (double)maxWidth / image.Width;
                    var ratioY = (double)maxHeight / image.Height;
                    var ratio = Math.Min(ratioX, ratioY);

                    var newWidth = (int)(image.Width * ratio);
                    var newHeight = (int)(image.Height * ratio);

                    image.Mutate(x => x.Resize(newWidth, newHeight));
                }

                var outputStream = new MemoryStream();

                // Save as WebP with quality settings
                var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder()
                {
                    Quality = 85, // Adjust quality as needed (0-100)
                    Method = SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Default
                };

                await image.SaveAsync(outputStream, encoder);

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"ImageSharp processing failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete a file from blob storage
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>Success status</returns>
        public async Task<bool> RemoveProductImageFileAsync(string fileName, string containerName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_productImageContainerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                var response = await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation("File {FileName} deleted: {Success}", fileName, response.Value);
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName}", fileName);
                return false;
            }
        }

        /// <summary>
        /// Get a file URL from blob storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>File URL or null if not found</returns>
        public async Task<string> GetFileUrlAsync(string fileName, string containerName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName ?? "_profileImageContainerName");
                var blobClient = containerClient.GetBlobClient(fileName);

                var exists = await blobClient.ExistsAsync();
                if (exists.Value)
                {
                    return blobClient.Uri.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL for file {FileName}", fileName);
                return null;
            }
        }

        /// <summary>
        /// Check if a file exists in blob storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>True if file exists</returns>
        public async Task<bool> FileExistsAsync(string fileName, string containerName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName ?? "_profileImageContainerName");
                var blobClient = containerClient.GetBlobClient(fileName);

                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if file {FileName} exists", fileName);
                return false;
            }
        }

        /// <summary>
        /// Upload a generic file to blob storage
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="fileName">Custom file name (optional)</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>Uploaded file URL</returns>
        public async Task<string> UploadFileAsync(IFormFile file, string fileName = null, string containerName = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was provided", nameof(file));
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName ?? "");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobName = fileName ?? $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = file.OpenReadStream();

                await blobClient.UploadAsync(stream, overwrite: true);

                // Set content type after upload
                await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                {
                    ContentType = file.ContentType ?? "application/octet-stream"
                });

                _logger.LogInformation("Successfully uploaded file {FileName}", blobName);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", fileName ?? file.FileName);
                throw;
            }
        }

        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>File metadata or null if not found</returns>
        public async Task<FileMetadata> GetFileMetadataAsync(string fileName, string containerName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName ?? "_profileImageContainerName");
                var blobClient = containerClient.GetBlobClient(fileName);

                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    return null;
                }

                var properties = await blobClient.GetPropertiesAsync();

                return new FileMetadata
                {
                    FileName = fileName,
                    Size = properties.Value.ContentLength,
                    ContentType = properties.Value.ContentType,
                    LastModified = properties.Value.LastModified.DateTime,
                    ETag = properties.Value.ETag.ToString(),
                    Url = blobClient.Uri.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for file {FileName}", fileName);
                return null;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validate if the uploaded file is a valid image
        /// </summary>
        /// <param name="file">File to validate</param>
        /// <returns>True if valid image</returns>
        private static bool IsValidImageFile(IFormFile file)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            return !string.IsNullOrEmpty(extension) && validExtensions.Contains(extension);
        }

        /// <summary>
        /// Calculate resize dimensions while maintaining aspect ratio
        /// </summary>
        /// <param name="originalWidth">Original image width</param>
        /// <param name="originalHeight">Original image height</param>
        /// <param name="maxWidth">Maximum allowed width</param>
        /// <param name="maxHeight">Maximum allowed height</param>
        /// <returns>New dimensions</returns>
        private static (int width, int height) CalculateResizeDimensions(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            if (originalWidth <= maxWidth && originalHeight <= maxHeight)
            {
                return (originalWidth, originalHeight);
            }

            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
        }

        /// <summary>
        /// Resize image with high quality settings
        /// </summary>
        /// <param name="image">Source image</param>
        /// <param name="width">Target width</param>
        /// <param name="height">Target height</param>
        /// <returns>Resized image</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(resized);

            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            graphics.DrawImage(image, 0, 0, width, height);
            return resized;
        }

        /// <summary>
        /// Compress image to JPEG with specified quality
        /// </summary>
        /// <param name="image">Image to compress</param>
        /// <param name="quality">JPEG quality (0-100)</param>
        /// <returns>Compressed image stream</returns>
        private static MemoryStream CompressImage(Image image, long quality)
        {
            var outputStream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1)
            {
                Param = { [0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality) }
            };

            image.Save(outputStream, encoder, encoderParams);
            outputStream.Position = 0;
            return outputStream;
        }

        /// <summary>
        /// Correct image orientation based on EXIF data
        /// </summary>
        /// <param name="image">Image to correct</param>
        private static void CorrectImageOrientation(Image image)
        {
            const int orientationPropertyId = 0x0112;

            if (!image.PropertyIdList.Contains(orientationPropertyId))
                return;

            var orientationProperty = image.GetPropertyItem(orientationPropertyId);
            var orientation = orientationProperty.Value[0];

            var rotateFlipType = orientation switch
            {
                3 => RotateFlipType.Rotate180FlipNone,
                6 => RotateFlipType.Rotate90FlipNone,
                8 => RotateFlipType.Rotate270FlipNone,
                _ => RotateFlipType.RotateNoneFlipNone
            };

            if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
            {
                image.RotateFlip(rotateFlipType);
                image.RemovePropertyItem(orientationPropertyId);
            }
        }

        #endregion
    }
}