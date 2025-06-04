using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Storage API operations
    /// </summary>
    public interface IStorageApi
    {
        /// <summary>
        /// Removes Exif data from an image to prevent privacy issues
        /// </summary>
        /// <param name="imageStream">Source image stream</param>
        /// <returns>Clean image stream without Exif data</returns>
        Task<MemoryStream> RemoveExifDataAsync(Stream imageStream);

        /// <summary>
        /// Process image before upload (resize, compress, remove Exif data)
        /// </summary>
        /// <param name="file">Image file from form upload</param>
        /// <param name="maxWidth">Maximum width (optional)</param>
        /// <param name="maxHeight">Maximum height (optional)</param>
        /// <returns>Processed image as a memory stream</returns>
        Task<MemoryStream> ProcessImageForUploadAsync(IFormFile file, int? maxWidth = null, int? maxHeight = null);

        /// <summary>
        /// Upload product image file to blob storage
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="type">File type (image, video, etc.)</param>
        /// <param name="formFile">Uploaded file</param>
        /// <param name="timeStamp">Optional timestamp</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateProductImageFileAsync(string productId, IFormFile formFile, TimeSpan? timeStamp = null);

        /// <summary>
        /// Upload product image file to blob storage
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="type">File type (image, video, etc.)</param>
        /// <param name="formFile">Uploaded file</param>
        /// <param name="timeStamp">Optional timestamp</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateVideoFileAsync(string productId, IFormFile formFile, TimeSpan? timeStamp = null);

        // Additional methods that could be useful for a complete storage API

        /// <summary>
        /// Delete a file from blob storage
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>Success status</returns>
        Task<bool> RemoveProductImageFileAsync(string fileName, string containerName = null);

        /// <summary>
        /// Delete a file from blob storage
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>Success status</returns>
        Task<bool> RemoveVideoFileAsync(string fileName, string containerName = null);

        /// <summary>
        /// Get a file URL from blob storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>File URL or null if not found</returns>
        Task<string> GetFileUrlAsync(string fileName, string containerName = null);

        /// <summary>
        /// Check if a file exists in blob storage
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>True if file exists</returns>
        Task<bool> FileExistsAsync(string fileName, string containerName = null);

        /// <summary>
        /// Upload a generic file to blob storage
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="fileName">Custom file name (optional)</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>Uploaded file URL</returns>
        Task<string> UploadFileAsync(IFormFile file, string fileName = null, string containerName = null);

        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="containerName">Container name (optional, uses default if not specified)</param>
        /// <returns>File metadata or null if not found</returns>
        Task<FileMetadata> GetFileMetadataAsync(string fileName, string containerName = null);
    }

    /// <summary>
    /// File metadata information
    /// </summary>
    public class FileMetadata
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime LastModified { get; set; }
        public string ETag { get; set; }
        public string Url { get; set; }
    }
}