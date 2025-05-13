using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using FFMpegCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Internal;
using System.Reflection.Metadata;
using Xabe.FFmpeg;
using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using Domain;

namespace ApiClient
{
    public class StorageAPI
    {

        string ughBlobConnectionString;
        string PostContainerName;
        string ProfileImageContainerName;
        string CourtImageContainerName;
        string ProductImageContainerName;
        string PrivateRunImageContainerName;
        string PostThumbnailImageContainerName;
        string TempVideoFileContainerName;
        private static readonly ErrorException _errorException = new ErrorException();
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Storage API
        /// </summary>
        /// <param name="configuration"></param>
        public StorageAPI(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.ughBlobConnectionString = Configuration.GetSection("BlobStorage")["ughBlobConnectionString"];
            this.PostContainerName = Configuration.GetSection("BlobStorage")["PostContainerName"];
            this.ProfileImageContainerName = Configuration.GetSection("BlobStorage")["ProfileContainerName"];
            this.PrivateRunImageContainerName = Configuration.GetSection("BlobStorage")["PrivateRunContainerName"];
            this.CourtImageContainerName = Configuration.GetSection("BlobStorage")["CourtContainerName"];
            this.ProductImageContainerName = Configuration.GetSection("BlobStorage")["ProductContainerName"];
            this.PostThumbnailImageContainerName = Configuration.GetSection("BlobStorage")["PostThumbnailContainerName"];
            this.TempVideoFileContainerName = Configuration.GetSection("BlobStorage")["TempVideoFileContainerName"];
        }

        /// <summary>
        /// Update PostFile
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="profileId"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePostFile(string postId, string profileId, string type, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            // Azure Storage setup
            string blobContainerName = TempVideoFileContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = string.Empty;

            // Get the file extension from the original file
            string fileExtension = Path.GetExtension(formFile.FileName);

            // Create a unique file name using the provided id and the original file extension
            uniqueFileName = $"{postId}{fileExtension}";

            // Run the upload task in the background
            return await Task.Run(async () =>
            {
                bool isUploaded = false;

                try
                {
                    // Create a blob client and container reference
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                    // Ensure the container is set to public access
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    // Get reference to the blob
                    var blob = blobContainer.GetBlockBlobReference(uniqueFileName);

                    if (type == "image")
                    {
                        // Convert the image to WebP format
                        var bitFile = ConvertImage.ConvertPngToWebP(type, formFile);
                        using (var stream = new MemoryStream(bitFile))
                        {
                            await blob.UploadFromStreamAsync(stream);
                        }
                    }
                    else if (type == "video")
                    {
                        // Read the file into a MemoryStream to avoid closing the original stream
                        using (var memoryStream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(memoryStream);
                            memoryStream.Position = 0; // Reset stream position to the beginning

                            // Upload the file stream
                            await blob.UploadFromStreamAsync(memoryStream);
                        }

                        isUploaded = true;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                    // Handle error exception logging here as needed
                }

                return isUploaded;
            });
        }

        /// <summary>
        /// Update PostImageFile
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="profileId"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePostImageFile(string postId, string profileId, string type, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            // Azure Storage setup
            string blobContainerName = PostContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = string.Empty;

            // Get the file extension from the original file
            string fileExtension = Path.GetExtension(formFile.FileName);

            // Create a unique file name using the provided id and the original file extension
            uniqueFileName = $"{postId}.webp";

            // Run the upload task in the background
            return await Task.Run(async () =>
            {
                bool isUploaded = false;

                try
                {
                    // Create a blob client and container reference
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                    // Ensure the container is set to public access
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    // Get reference to the blob
                    var blob = blobContainer.GetBlockBlobReference(uniqueFileName);

                    if (type == "image")
                    {
                        if (formFile != null && formFile.Length > 0)
                        {
                            using (var stream = new MemoryStream())
                            {
                                // Copy the uploaded file to the stream
                                await formFile.CopyToAsync(stream);
                                stream.Position = 0;

                                // Load the image and correct its orientation using System.Drawing
                                using (var image = Image.FromStream(stream))
                                {
                                    var correctedImage = RotateImage(image, GetOrientation(image));

                                    // Convert corrected image to WebP using SkiaSharp
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        using (SKBitmap skBitmap = ConvertToSkiaBitmap(correctedImage))
                                        {
                                            // Resize the image to the specified width and height
                                            using (SKBitmap resizedBitmap = skBitmap.Resize(new SKImageInfo(800, 409), SKFilterQuality.Medium))
                                            using (SKImage skImage = SKImage.FromBitmap(resizedBitmap))
                                            using (SKData encoded = skImage.Encode(SKEncodedImageFormat.Webp, 100))
                                            {
                                                // Write the encoded data to the memory stream
                                                encoded.SaveTo(memoryStream);
                                                memoryStream.Position = 0;

                                                // Upload the WebP image to Azure Blob Storage
                                                await blob.UploadFromStreamAsync(memoryStream);
                                                isUploaded = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                }

                return isUploaded;
            });
        }

        /// <summary>
        /// Update PostFile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> UpdatePostFile(string id, string type, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            // Azure Storage setup
            string blobContainerName = PostContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = string.Empty;
            string tempPath = Path.GetTempPath(); // Temporary file storage location
            string outputPath = string.Empty;     // Path for the converted video
            string filePathError = string.Empty;

            if (type == "image")
            {
                uniqueFileName = $"{id}.webp"; // Using .webp for image
            }
            else if (type == "video")
            {
                uniqueFileName = $"{id}.mp4";  // Using .mp4 for video
            }

            bool isUploaded = false;

            try
            {
                // Create a blob client and container reference
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                // Ensure the container is set to public access
                await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

                // Get reference to the blob
                var blob = blobContainer.GetBlockBlobReference(uniqueFileName);

                if (type == "image")
                {
                    // Convert the image to WebP format
                    var bitFile = ConvertImage.ConvertPngToWebP(type, formFile);
                    using (var stream = new MemoryStream(bitFile))
                    {
                        await blob.UploadFromStreamAsync(stream);
                    }
                }
                else if (type == "video")
                {
                    // Ensure the video file is not null
                    if (formFile == null || formFile.Length == 0)
                    {
                        throw new ArgumentException("Video file is required.");
                    }

                    // Define the paths for uploaded video and thumbnail
                    var uploadsVideoFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnail", "thumbnail_tmp");
                    var fileName = Path.GetFileName(formFile.FileName);
                    var filePath = Path.Combine(uploadsVideoFolder, fileName);
                    filePathError = filePath;

                    // Save the file to the specified path
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    // Generate thumbnail using FFmpeg
                    string thumbnailDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnail", "thumbnail_tmp");
                    string thumbnailPath = Path.Combine(thumbnailDirectory, $"{id}.jpg"); // Specify the file name for thumbnail
                    await UpdatePostThumbnailImage(thumbnailPath);
                    // Ensure thumbnail directory exists
                    if (!Directory.Exists(thumbnailDirectory))
                    {
                        Directory.CreateDirectory(thumbnailDirectory);
                    }


                    // Convert video to MP4 format
                    string convertedMp4File = await ConvertVideoFile.ConvertMovToMp4Async(filePath, "");
                    if (string.IsNullOrEmpty(convertedMp4File))
                    {
                        throw new Exception("Converted MP4 file path is null or empty.");
                    }

                    // Upload the converted video stream to Azure Blob Storage
                    using (var stream = new FileStream(convertedMp4File, FileMode.Open))
                    {
                        await blob.UploadFromStreamAsync(stream);
                    }



                    // Set the timestamp to 00:00:01 if not provided
                    string timeStampString = timeStamp.HasValue ? timeStamp.Value.ToString(@"hh\:mm\:ss") : "00:00:01";
                    string ffmpegArgs = $"-ss {timeStampString} -i \"{convertedMp4File}\" -frames:v 1 \"{thumbnailPath}\"";



                    await Task.Run(async () =>
                    {
                        using (Process ffmpegProcess = new Process())
                        {
                            ffmpegProcess.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin", "ffmpeg.exe");
                            ffmpegProcess.StartInfo.Arguments = ffmpegArgs;
                            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                            ffmpegProcess.StartInfo.RedirectStandardError = true;
                            ffmpegProcess.StartInfo.UseShellExecute = false;
                            ffmpegProcess.StartInfo.CreateNoWindow = true;

                            // Start the FFmpeg process
                            ffmpegProcess.Start();

                            // Asynchronously wait for the process to exit
                            await ffmpegProcess.WaitForExitAsync();

                            // Check if the process exited successfully
                            if (ffmpegProcess.ExitCode != 0)
                            {
                                string errorOutput = await ffmpegProcess.StandardError.ReadToEndAsync();
                                Console.WriteLine($"FFmpeg Error: {errorOutput}");
                                throw new Exception("FFmpeg process failed with exit code " + ffmpegProcess.ExitCode);
                            }

                            Console.WriteLine("FFmpeg process completed successfully.");
                        }
                    });

                    isUploaded = true;



                    _errorException.ErrorMessage = "Method: UpdatePostFile";
                    _errorException.DetailMessage = "";
                    _errorException.ProfileId = id;
                    _errorException.Status = "Successed";

                   

                    // Clean up temporary files
                    if (File.Exists(filePath)) File.Delete(filePath);
                    if (File.Exists(convertedMp4File)) File.Delete(convertedMp4File);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                _errorException.ErrorMessage = ex.Message + " Method:UpdatePostFile / " + "OutPut:" + outputPath + "/" + "FilePath:" + filePathError;
                _errorException.DetailMessage = ex.InnerException?.ToString();
                _errorException.ProfileId = id;
                _errorException.Status = "Error";

               
            }

            return isUploaded;
        }

        /// <summary>
        /// Delete PostFile From BlobStorage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task DeletePostFileFromBlobStorage(string fileName)
        {
            string blobContainerName = PostContainerName;
            string connectionString = ughBlobConnectionString;

            try
            {
                // Create a blob client and container reference
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                // Get reference to the blob
                var blob = blobContainer.GetBlockBlobReference(fileName);

                // Delete the blob if it exists
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting blob from storage: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete PostFile From BlobStorage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task DeleteCourtImageFromBlobStorage(string courtId)
        {
            string blobContainerName = CourtImageContainerName;
            string connectionString = ughBlobConnectionString;

            try
            {
                // Create a blob client and container reference
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                // Get reference to the blob
                var blob = blobContainer.GetBlockBlobReference(courtId+".webp");

                // Delete the blob if it exists
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting blob from storage: {ex.Message}");
            }
        }

        /// <summary>
        /// Update ProfileImage
        /// </summary>
        /// <param name="id"></param>
        /// <param name="formFile"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProfileImage(string id, IFormFile formFile)
        {
            // Azure Storage setup
            string blobContainerName = ProfileImageContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = $"{id}.webp";

            bool isUploaded = false;

            try
            {
                // Create a blob client and container reference
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                // Ensure the container is set to public access
                await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

                
                // Read image and rotate if needed
                using (var stream = formFile.OpenReadStream())
                using (var image = Image.FromStream(stream))
                {
                    RotateImageIfNeeded(image);  // Correct rotation
                    using (var memoryStream = new MemoryStream())
                    {
                        image.Save(memoryStream, ImageFormat.Webp); // Convert to WebP format
                        memoryStream.Position = 0;

                        // Upload the image to Blob Storage
                        var blobx = blobContainer.GetBlockBlobReference(uniqueFileName);
                        await blobx.UploadFromStreamAsync(memoryStream);
                    }
                }



                isUploaded = true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
            }

            return isUploaded;
        }

        private static void RotateImageIfNeeded(Image image)
        {
            if (Array.IndexOf(image.PropertyIdList, 274) > -1)
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];
                RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;

                switch (orientation)
                {
                    case 3: rotateFlipType = RotateFlipType.Rotate180FlipNone; break;
                    case 6: rotateFlipType = RotateFlipType.Rotate90FlipNone; break;
                    case 8: rotateFlipType = RotateFlipType.Rotate270FlipNone; break;
                }

                if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(rotateFlipType);
                    image.RemovePropertyItem(274); // Remove EXIF tag after fixing
                }
            }
        }

        /// <summary>
        /// Update PostThumbnailImage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePostThumbnailImage(string fileName)
        {
            // Azure Storage setup
            string blobContainerName = PostThumbnailImageContainerName; // Ensure this variable is defined
            string connectionString = ughBlobConnectionString;          // Ensure this variable is defined
            bool isUploaded = false;

            try
            {
                // Create a blob client and container reference
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                // Local path to the thumbnail file
                string localFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnail", "thumbnail_tmp", fileName);

                // Ensure the container is set to public access
                await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

                // Get reference to the blob
                var blob = blobContainer.GetBlockBlobReference(fileName);

                // Open the local file and upload its data
                using (var stream = File.OpenRead(localFilePath))
                {
                    await blob.UploadFromStreamAsync(stream);
                }

                // Delete the file from the local directory after upload
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                    Console.WriteLine($"File {fileName} deleted from local storage.");
                }

                isUploaded = true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
            }

            _errorException.ErrorMessage = fileName;

            _errorException.Status = "uploaded to blob";

           


            return isUploaded;
        }


        /// <summary>
        /// Delete temp PostFile From BlobStorage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task DeletetempPostFileFromBlobStorage(string fileName)
        {
            string blobContainerName = TempVideoFileContainerName;
            string connectionString = ughBlobConnectionString;

            try
            {
                // Create a blob client and container reference
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                // Get reference to the blob
                var blob = blobContainer.GetBlockBlobReference(fileName);

                // Delete the blob if it exists
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting blob from storage: {ex.Message}");
            }
        }


        /// <summary>
        /// Upload VideoAsync
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="id"></param>
        /// <param name="blob"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task UploadVideoAsync(IFormFile formFile, string id, CloudBlockBlob blob, TimeSpan? timeStamp)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("Video file is required.");
            }

            var uploadsVideoFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var fileName = Path.GetFileName(formFile.FileName);
            var filePath = Path.Combine(uploadsVideoFolder, fileName);

            // Save the file to the specified path
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }

            // Ensure thumbnail directory exists
            string thumbnailDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnail", "thumbnail_tmp");
            Directory.CreateDirectory(thumbnailDirectory);

            string thumbnailPath = Path.Combine(thumbnailDirectory, $"{id}.jpg"); // Specify the file name for thumbnail


            // Convert video to MP4 format
            string convertedMp4File = await ConvertVideoFile.ConvertMovToMp4Async(filePath, "");
            if (string.IsNullOrEmpty(convertedMp4File))
            {
                throw new Exception("Converted MP4 file path is null or empty.");
            }

            // Upload the converted video stream to Azure Blob Storage
            using (var stream = new FileStream(convertedMp4File, FileMode.Open))
            {
                await blob.UploadFromStreamAsync(stream);
            }

            // Generate thumbnail
            string timeStampString = timeStamp?.ToString(@"hh\:mm\:ss") ?? "00:00:01";
            await GenerateThumbnailAsync(convertedMp4File, thumbnailPath, timeStampString);
            await UpdatePostThumbnailImage(thumbnailPath);
            // Clean up temporary files
            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(convertedMp4File)) File.Delete(convertedMp4File);
        }


        /// <summary>
        /// Generate ThumbnailAsync
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <param name="thumbnailPath"></param>
        /// <param name="timeStampString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task GenerateThumbnailAsync(string videoFilePath, string thumbnailPath, string timeStampString)
        {
            string ffmpegArgs = $"-ss {timeStampString} -i \"{videoFilePath}\" -frames:v 1 \"{thumbnailPath}\"";

            await Task.Run(() =>
            {
                using (var ffmpegProcess = new Process())
                {
                    ffmpegProcess.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin", "ffmpeg.exe");
                    ffmpegProcess.StartInfo.Arguments = ffmpegArgs;
                    ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                    ffmpegProcess.StartInfo.RedirectStandardError = true;
                    ffmpegProcess.StartInfo.UseShellExecute = false;
                    ffmpegProcess.StartInfo.CreateNoWindow = true;

                    // Start the FFmpeg process
                    ffmpegProcess.Start();

                    // Wait for the process to exit
                    ffmpegProcess.WaitForExit();

                    // Check if the process exited successfully
                    if (ffmpegProcess.ExitCode != 0)
                    {
                        string errorOutput = ffmpegProcess.StandardError.ReadToEnd();
                        throw new Exception($"FFmpeg process failed with exit code {ffmpegProcess.ExitCode}: {errorOutput}");
                    }
                }
            });
        }

        /// <summary>
        /// Convert To SkiaBitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private SKBitmap ConvertToSkiaBitmap(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Save as PNG
                ms.Seek(0, SeekOrigin.Begin); // Reset stream position
                return SKBitmap.Decode(ms); // Decode using SkiaSharp
            }
        }


        /// <summary>
        /// Log Success
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task LogSuccess(string id)
        {
            // Implement logging of success
            _errorException.ErrorMessage = "Method: UpdatePostFile";
            _errorException.DetailMessage = "";
            _errorException.ProfileId = id;
            _errorException.Status = "Succeeded";

          
        }


        /// <summary>
        /// Rotate Image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private Image RotateImage(Image img, int orientation)
        {
            switch (orientation)
            {
                case 1: // Normal
                    return img; // No rotation needed
                case 3: // 180 degrees
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 6: // 90 degrees clockwise
                    img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 8: // 270 degrees clockwise
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            return img;
        }

        /// <summary>
        /// Get Orientation
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private int GetOrientation(Image image)
        {
            const int orientationId = 0x0112; // EXIF tag for orientation
            if (image.PropertyIdList.Contains(orientationId))
            {
                var prop = image.GetPropertyItem(orientationId);
                return prop.Value[0]; // Return the orientation value
            }
            return 1; // Default orientation (1 = Normal)
        }

        /// <summary>
        /// Handle Error
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task HandleError(string id, Exception ex)
        {
            // Handle and log the error
            _errorException.ErrorMessage = "Method: UpdatePostFile";
            _errorException.DetailMessage = ex.Message;
            _errorException.ProfileId = id;
            _errorException.Status = "Failed";

            
        }



        /// <summary>
        /// Update CourtImageFile
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="profileId"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public async Task<bool> UpdateCourtImageFile(string courtId,  string type, IFormFile formFile)
        {
            // Azure Storage setup
            string blobContainerName = CourtImageContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = string.Empty;

            // Get the file extension from the original file
            string fileExtension = Path.GetExtension(formFile.FileName);

            // Create a unique file name using the provided id and the original file extension
            uniqueFileName = $"{courtId}.webp";

            // Run the upload task in the background
            return await Task.Run(async () =>
            {
                bool isUploaded = false;

                try
                {
                    // Create a blob client and container reference
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                    // Ensure the container is set to public access
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    // Get reference to the blob
                    var blob = blobContainer.GetBlockBlobReference(uniqueFileName);

                    if (type == "image")
                    {
                        if (formFile != null && formFile.Length > 0)
                        {
                            using (var stream = new MemoryStream())
                            {
                                // Copy the uploaded file to the stream
                                await formFile.CopyToAsync(stream);
                                stream.Position = 0;

                                // Load the image and correct its orientation using System.Drawing
                                using (var image = Image.FromStream(stream))
                                {
                                    var correctedImage = RotateImage(image, GetOrientation(image));

                                    // Convert corrected image to WebP using SkiaSharp
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        using (SKBitmap skBitmap = ConvertToSkiaBitmap(correctedImage))
                                        {
                                            // Resize the image to the specified width and height
                                            using (SKBitmap resizedBitmap = skBitmap.Resize(new SKImageInfo(800, 409), SKFilterQuality.Medium))
                                            using (SKImage skImage = SKImage.FromBitmap(resizedBitmap))
                                            using (SKData encoded = skImage.Encode(SKEncodedImageFormat.Webp, 100))
                                            {
                                                // Write the encoded data to the memory stream
                                                encoded.SaveTo(memoryStream);
                                                memoryStream.Position = 0;

                                                // Upload the WebP image to Azure Blob Storage
                                                await blob.UploadFromStreamAsync(memoryStream);
                                                isUploaded = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                }

                return isUploaded;
            });
        }


        /// <summary>
        /// Update CourtImageFile
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="profileId"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProductImageFile(string productId, string type, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            // Azure Storage setup
            string blobContainerName = ProductImageContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = string.Empty;

            // Get the file extension from the original file
            string fileExtension = Path.GetExtension(formFile.FileName);

            // Create a unique file name using the provided id and the original file extension
            uniqueFileName = $"{productId}.webp";

            // Run the upload task in the background
            return await Task.Run(async () =>
            {
                bool isUploaded = false;

                try
                {
                    // Create a blob client and container reference
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                    // Ensure the container is set to public access
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    // Get reference to the blob
                    var blob = blobContainer.GetBlockBlobReference(uniqueFileName);

                    if (type == "image")
                    {
                        if (formFile != null && formFile.Length > 0)
                        {
                            using (var stream = new MemoryStream())
                            {
                                // Copy the uploaded file to the stream
                                await formFile.CopyToAsync(stream);
                                stream.Position = 0;

                                // Load the image and correct its orientation using System.Drawing
                                using (var image = Image.FromStream(stream))
                                {
                                    var correctedImage = RotateImage(image, GetOrientation(image));

                                    // Convert corrected image to WebP using SkiaSharp
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        using (SKBitmap skBitmap = ConvertToSkiaBitmap(correctedImage))
                                        {
                                            // Resize the image to the specified width and height
                                            using (SKBitmap resizedBitmap = skBitmap.Resize(new SKImageInfo(800, 409), SKFilterQuality.Medium))
                                            using (SKImage skImage = SKImage.FromBitmap(resizedBitmap))
                                            using (SKData encoded = skImage.Encode(SKEncodedImageFormat.Webp, 100))
                                            {
                                                // Write the encoded data to the memory stream
                                                encoded.SaveTo(memoryStream);
                                                memoryStream.Position = 0;

                                                // Upload the WebP image to Azure Blob Storage
                                                await blob.UploadFromStreamAsync(memoryStream);
                                                isUploaded = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                }

                return isUploaded;
            });
        }


        /// <summary>
        /// Update CourtImageFile
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="profileId"></param>
        /// <param name="type"></param>
        /// <param name="formFile"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePrivateRunImageFile(string PrivateRunId, string type, IFormFile formFile, TimeSpan? timeStamp = null)
        {
            // Azure Storage setup
            string blobContainerName = PrivateRunImageContainerName;
            string connectionString = ughBlobConnectionString;
            string uniqueFileName = string.Empty;

            // Get the file extension from the original file
            string fileExtension = Path.GetExtension(formFile.FileName);

            // Create a unique file name using the provided id and the original file extension
            uniqueFileName = $"{PrivateRunId}.webp";

            // Run the upload task in the background
            return await Task.Run(async () =>
            {
                bool isUploaded = false;

                try
                {
                    // Create a blob client and container reference
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                    // Ensure the container is set to public access
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    // Get reference to the blob
                    var blob = blobContainer.GetBlockBlobReference(uniqueFileName);

                    if (type == "image")
                    {
                        if (formFile != null && formFile.Length > 0)
                        {
                            using (var stream = new MemoryStream())
                            {
                                // Copy the uploaded file to the stream
                                await formFile.CopyToAsync(stream);
                                stream.Position = 0;

                                // Load the image and correct its orientation using System.Drawing
                                using (var image = Image.FromStream(stream))
                                {
                                    var correctedImage = RotateImage(image, GetOrientation(image));

                                    // Convert corrected image to WebP using SkiaSharp
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        using (SKBitmap skBitmap = ConvertToSkiaBitmap(correctedImage))
                                        {
                                            // Resize the image to the specified width and height
                                            using (SKBitmap resizedBitmap = skBitmap.Resize(new SKImageInfo(800, 409), SKFilterQuality.Medium))
                                            using (SKImage skImage = SKImage.FromBitmap(resizedBitmap))
                                            using (SKData encoded = skImage.Encode(SKEncodedImageFormat.Webp, 100))
                                            {
                                                // Write the encoded data to the memory stream
                                                encoded.SaveTo(memoryStream);
                                                memoryStream.Position = 0;

                                                // Upload the WebP image to Azure Blob Storage
                                                await blob.UploadFromStreamAsync(memoryStream);
                                                isUploaded = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading to blob storage: {ex.Message}");
                }

                return isUploaded;
            });
        }

    }
}

