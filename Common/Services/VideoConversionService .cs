using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Domain; // Reference to Domain for AppException

namespace Common.Services
{
    /// <summary>
    /// Service to handle video conversion operations with proper error handling and logging
    /// </summary>
    public class VideoConversionService : IVideoConversionService
    {
        private readonly ILogger<VideoConversionService> _logger; // Fixed: Using correct type parameter
        private readonly string _ffmpegDirectory;

        public VideoConversionService(ILogger<VideoConversionService> logger, string ffmpegBasePath = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Use provided path or default to application directory
            _ffmpegDirectory = ffmpegBasePath ??
                Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin");

            // Set FFmpeg path once during initialization
            FFmpeg.SetExecutablesPath(_ffmpegDirectory);
        }

        /// <summary>
        /// Converts video files to MP4 format
        /// </summary>
        /// <param name="inputFilePath">Path to the input video file</param>
        /// <param name="outputFolder">Folder to save the converted file</param>
        /// <param name="cancellationToken">Cancellation token to abort operation</param>
        /// <returns>Path to the converted file, or null if conversion failed</returns>
        public async Task<string> ConvertToMp4Async(
            string inputFilePath,
            string outputFolder,
            CancellationToken cancellationToken = default)
        {
            ValidateParameters(inputFilePath, outputFolder);

            try
            {
                _logger.LogInformation("Starting video conversion for {FilePath}", inputFilePath);

                // Ensure output folder exists
                Directory.CreateDirectory(outputFolder);

                // If input file is already MP4, simply copy it to the output folder
                if (inputFilePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    return await CopyMp4FileAsync(inputFilePath, outputFolder);
                }

                return await PerformConversionAsync(inputFilePath, outputFolder, cancellationToken);
            }
            catch (Exception ex) when (IsFileSystemException(ex))
            {
                _logger.LogError(ex, "File system error during video conversion");
                throw new AppException("Unable to access file during conversion", ex);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Video conversion was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during video conversion");
                return null;
            }
        }

        private void ValidateParameters(string inputFilePath, string outputFolder)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("Input file path cannot be empty", nameof(inputFilePath));

            if (string.IsNullOrEmpty(outputFolder))
                throw new ArgumentException("Output folder cannot be empty", nameof(outputFolder));

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input file not found", inputFilePath);
        }

        private async Task<string> CopyMp4FileAsync(string inputFilePath, string outputFolder)
        {
            _logger.LogInformation("File is already MP4, copying to output folder");
            var outputFileName = Path.GetFileName(inputFilePath);
            var outputFilePath = Path.Combine(outputFolder, outputFileName);

            await CopyFileWithRetryAsync(inputFilePath, outputFilePath);
            return outputFilePath;
        }

        private async Task<string> PerformConversionAsync(
            string inputFilePath,
            string outputFolder,
            CancellationToken cancellationToken)
        {
            // Create the output file path
            var outputFileNameMp4 = Path.GetFileNameWithoutExtension(inputFilePath) + ".mp4";
            var outputFilePathMp4 = Path.Combine(outputFolder, outputFileNameMp4);

            // Get media info for logging
            var mediaInfo = await FFmpeg.GetMediaInfo(inputFilePath, cancellationToken);
            _logger.LogInformation(
                "Converting video: Duration {Duration}, Size {Size}",
                mediaInfo.Duration,
                new FileInfo(inputFilePath).Length);

            // Create conversion with progress reporting
            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputFilePath}\"")
                .SetOutput(outputFilePathMp4);

            // Add progress handler for logging
            conversion.OnProgress += (sender, args) =>
            {
                if (args.Percent % 10 == 0) // Log every 10%
                {
                    _logger.LogInformation(
                        "Conversion progress: {Percent}%, Time: {Time}",
                        args.Percent,
                        args.ProcessedDuration); // Using ProcessedDuration for Xabe.FFmpeg 6.0.1
                }
            };

            // Start conversion
            await conversion.Start(cancellationToken);

            _logger.LogInformation("Video conversion completed successfully: {OutputPath}", outputFilePathMp4);
            return outputFilePathMp4;
        }

        /// <summary>
        /// Copies a file with retry logic to handle temporary file locks
        /// </summary>
        private async Task CopyFileWithRetryAsync(
            string sourceFilePath,
            string destinationFilePath,
            int maxRetries = 3,
            int delayMilliseconds = 1000)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using (var sourceStream = new FileStream(
                        sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var destinationStream = new FileStream(
                        destinationFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                    return; // Success, exit the method
                }
                catch (IOException ex)
                {
                    lastException = ex;

                    if (attempt < maxRetries - 1)
                    {
                        _logger.LogWarning(
                            "File is being used by another process, retry {Attempt}/{MaxRetries}",
                            attempt + 1, maxRetries);

                        await Task.Delay(delayMilliseconds * (attempt + 1)); // Progressive delay
                    }
                }
            }

            throw new IOException(
                $"Unable to copy file after {maxRetries} attempts: {sourceFilePath}",
                lastException);
        }

        private bool IsFileSystemException(Exception ex)
        {
            return ex is IOException ||
                   ex is UnauthorizedAccessException ||
                   ex is System.Security.SecurityException;
        }
    }
}