using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Common.Services
{
    /// <summary>
    /// Centralized service for storage operations
    /// </summary>
    public class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IVideoConversionService _videoConverter;
        private readonly string _storageConnectionString;
        private readonly string _storageBaseUrl;

        public StorageService(
            ILogger<StorageService> logger,
            IConfiguration configuration,
            IVideoConversionService videoConverter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _videoConverter = videoConverter ?? throw new ArgumentNullException(nameof(videoConverter));

            _storageConnectionString = _configuration["Storage:ConnectionString"];
            _storageBaseUrl = _configuration["Storage:BaseUrl"] ?? "https://uhblobstorageaccount.blob.core.windows.net";

            if (string.IsNullOrEmpty(_storageConnectionString))
            {
                _logger.LogWarning("Storage connection string is not configured");
            }
        }

        /// <summary>
        /// Upload a file to blob storage
        /// </summary>
        public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string containerName)
        {
            if (fileData == null || fileData.Length == 0)
            {
                _logger.LogWarning("Empty file data provided for upload");
                throw new ArgumentException("File data cannot be empty", nameof(fileData));
            }

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(containerName))
            {
                _logger.LogWarning("Missing file name or container name");
                throw new ArgumentException("File name and container name are required");
            }

            try
            {
                // Implement your blob storage upload here
                // For now, we'll just simulate the upload and return the URL

                // In a real implementation, you would use Azure.Storage.Blobs or similar
                await Task.Delay(100); // Simulate upload

                return $"{_storageBaseUrl}/{containerName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {fileName} to container {containerName}");
                throw new ApplicationException($"Error uploading file to storage", ex);
            }
        }

        /// <summary>
        /// Upload a stream to blob storage
        /// </summary>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Empty file stream provided for upload");
                throw new ArgumentException("File stream cannot be empty", nameof(fileStream));
            }

            // Convert stream to byte array
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            return await UploadFileAsync(memoryStream.ToArray(), fileName, containerName);
        }

        /// <summary>
        /// Generate a video thumbnail and upload it to storage
        /// </summary>
        public async Task<string> GenerateVideoThumbnailAsync(IFormFile videoFile, string thumbnailFileName, string containerName)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                _logger.LogWarning("Empty video file provided for thumbnail generation");
                throw new ArgumentException("Video file cannot be empty", nameof(videoFile));
            }

            try
            {
                // Save the uploaded video file to a temporary location
                var tempDir = Path.Combine(Path.GetTempPath(), "videothumbnails");
                Directory.CreateDirectory(tempDir);

                var tempVideoPath = Path.Combine(tempDir, Path.GetFileName(videoFile.FileName));
                var tempThumbnailPath = Path.Combine(tempDir, $"{Path.GetFileNameWithoutExtension(videoFile.FileName)}_thumb.png");

                using (var stream = new FileStream(tempVideoPath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                // Generate thumbnail using FFmpeg
                var mediaInfo = await FFmpeg.GetMediaInfo(tempVideoPath);
                var duration = mediaInfo.Duration;

                // Take thumbnail at 1 second or 10% of the video, whichever is less
                var thumbnailTime = TimeSpan.FromSeconds(Math.Min(1, duration.TotalSeconds * 0.1));

                var conversion = await FFmpeg.Conversions.New()
                    .AddParameter($"-ss {thumbnailTime}")
                    .AddParameter($"-i \"{tempVideoPath}\"")
                    .AddParameter("-vframes 1")
                    .AddParameter($"\"{tempThumbnailPath}\"")
                    .Start();

                // Upload thumbnail to storage
                using var thumbnailStream = new FileStream(tempThumbnailPath, FileMode.Open);
                var thumbnailUrl = await UploadFileAsync(thumbnailStream, thumbnailFileName, containerName);

                // Clean up temporary files
                try
                {
                    File.Delete(tempVideoPath);
                    File.Delete(tempThumbnailPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error cleaning up temporary files");
                }

                return thumbnailUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating video thumbnail");
                throw new ApplicationException("Error generating video thumbnail", ex);
            }
        }
    }

    public interface IStorageService
    {
        Task<string> UploadFileAsync(byte[] fileData, string fileName, string containerName);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
        Task<string> GenerateVideoThumbnailAsync(IFormFile videoFile, string thumbnailFileName, string containerName);
    }
}