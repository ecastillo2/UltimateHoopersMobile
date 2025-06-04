using Common;
using Common.Utilities;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using WebAPI.ApiClients;
using Website.Models;
using Website.ViewModels;

namespace Web.Controllers
{
    public class VideoController : Controller
    {
        private readonly IVideoApi _videoApi;
        private readonly IStorageApi _storageApi;
        private readonly ILogger<VideoController> _logger;

        public VideoController(IStorageApi storageApi, IVideoApi videoApi, ILogger<VideoController> logger)
        {
            _storageApi = storageApi ?? throw new ArgumentNullException(nameof(storageApi));
            _videoApi = videoApi ?? throw new ArgumentNullException(nameof(videoApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Video(string cursor = null, int limit = 10, string direction = "next", string sortBy = "Title", CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view Videos.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                var result = await _videoApi.GetVideosWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                var viewModel = new VideosViewModel
                {
                    VideoList = result.Items,
                    NextCursor = result.NextCursor,
                    CurrentSortBy = sortBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Videos");
                TempData["Error"] = "An error occurred while retrieving Videos. Please try again later.";
                return RedirectToAction("Dashboard", "Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVideoData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Video ID is required" });
                }

                var video = await _videoApi.GetVideoByIdAsync(id, accessToken, cancellationToken);
                if (video == null)
                {
                    return Json(new { success = false, message = "Video not found" });
                }

                var videoData = new
                {
                    success = true,
                    video = new
                    {
                        videoId = video.VideoId,
                        title = video.Title,
                    
                       
                    }
                };

                return Json(videoData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Video data for ID: {videoId}", id);
                return Json(new { success = false, message = "Error loading Video data: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view Video details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var video = await _videoApi.GetVideoByIdAsync(id, accessToken, cancellationToken);
                if (video == null)
                {
                    TempData["Error"] = "Video not found.";
                    return RedirectToAction("Video");
                }

                return View(video);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Video details for ID: {VideoId}", id);
                TempData["Error"] = "An error occurred while retrieving Video details. Please try again later.";
                return RedirectToAction("Video");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a Product.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Video video, IFormFile ImageFile, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                // Enhanced validation
                var validationResult = ValidateVideo(video);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage, field = validationResult.Field });
                }

                // Validate image file if provided
                if (ImageFile != null)
                {
                    var fileValidation = ValidateImageFile(ImageFile);
                    if (!fileValidation.IsValid)
                    {
                        return Json(new { success = false, message = fileValidation.ErrorMessage });
                    }
                }

                // Set default values
                video.VideoId = Guid.NewGuid().ToString();
                video.VideoNumber = UniqueIdNumber.GenerateSixDigit();
                video.Status = video.Status ?? "Active";
               

                // Handle image file upload
                string imageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        video.VideoURL = video.VideoId + Path.GetExtension(ImageFile.FileName).ToLower();
                        var uploadResult = await _storageApi.UpdateVideoFileAsync(video.VideoId, ImageFile);

                        if (uploadResult)
                        {
                            // Set the image URL - you may need to construct this based on your storage configuration
                            imageUrl = $"/api/storage/video/{video.VideoURL}"; // Adjust based on your storage setup
                            video.VideoURL = imageUrl;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to upload image. Please try again." });
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading image for Video: {VideoId}", video.VideoId);
                        return Json(new { success = false, message = "Error uploading image: " + uploadEx.Message });
                    }
                }
                else if (!string.IsNullOrEmpty(video.VideoURL))
                {
                    // Validate image URL if provided
                    var urlValidation = await ValidateVideoUrl(video.VideoURL);
                    if (!urlValidation.IsValid)
                    {
                        return Json(new { success = false, message = urlValidation.ErrorMessage });
                    }
                }

                // Create Video
                var createdVideo = await _videoApi.CreateVideoAsync(video, accessToken, cancellationToken);

                if (createdVideo != null)
                {
                    _logger.LogInformation("Video created successfully: {VideoId}", video.VideoId);

                    return Json(new
                    {
                        success = true,
                        message = "Video created successfully!",
                        video = new
                        {
                            videoId = video.VideoId,
                            title = video.Title,
                            videoNumber = video.VideoNumber,
                            ClientId = video.ClientId,
                            VideoName = video.VideoName,
                            VideoURL = video.VideoURL,
                            VideoDate = video.VideoDate,
                            Status = video.Status,
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create Video. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Video");
                return Json(new { success = false, message = "An error occurred while creating the Video: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a Video.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var video = await _videoApi.GetVideoByIdAsync(id, accessToken, cancellationToken);
                if (video == null)
                {
                    TempData["Error"] = "Video not found.";
                    return RedirectToAction("Video");
                }

                return View(video);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Video for edit, ID: {VideoId}", id);
                TempData["Error"] = "An error occurred while retrieving the Video. Please try again later.";
                return RedirectToAction("Video");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Video video, IFormFile VideoFile, bool RemoveImage = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(video.VideoId))
                {
                    return Json(new { success = false, message = "Video ID is required" });
                }

                // Enhanced validation
                var validationResult = ValidateVideo(video);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage, field = validationResult.Field });
                }

                // Get existing Video
                var existingVideo = await _videoApi.GetVideoByIdAsync(video.VideoId, accessToken, cancellationToken);
                if (existingVideo == null)
                {
                    return Json(new { success = false, message = "Video not found" });
                }

                // Handle image operations
                string videoUrl = existingVideo.VideoURL;

                if (RemoveImage)
                {
                    // Remove image
                    videoUrl = null;
                    video.VideoURL = null;
                }
                else if (VideoFile != null && VideoFile.Length > 0)
                {
                    // Validate and upload new image
                    var fileValidation = ValidateImageFile(VideoFile);
                    if (!fileValidation.IsValid)
                    {
                        return Json(new { success = false, message = fileValidation.ErrorMessage });
                    }

                    try
                    {
                        var uploadResult = await _storageApi.UpdateVideoFileAsync(video.VideoId, VideoFile);
                        if (uploadResult)
                        {
                            videoUrl = $"/api/storage/Video/{video.VideoId}{Path.GetExtension(VideoFile.FileName).ToLower()}";
                            video.VideoURL = videoUrl;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to upload image. Please try again." });
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading image for video: {VideoId}", video.VideoId);
                        return Json(new { success = false, message = "Error uploading image: " + uploadEx.Message });
                    }
                }
                else if (!string.IsNullOrEmpty(video.VideoURL) && video.VideoURL != existingVideo.VideoURL)
                {
                    // Validate new image URL
                    var urlValidation = await ValidateVideoUrl(video.VideoURL);
                    if (!urlValidation.IsValid)
                    {
                        return Json(new { success = false, message = urlValidation.ErrorMessage });
                    }
                    videoUrl = video.VideoURL;
                }
                else
                {
                    // Keep existing image
                    video.VideoURL = existingVideo.VideoURL;
                }

                // Set default values
                video.Status = video.Status ?? "Active";
                video.VideoNumber = existingVideo.VideoNumber; // Preserve original Video number

                // Update Video
                await _videoApi.UpdateVideoFileAsync(video, accessToken, cancellationToken);

                _logger.LogInformation("Video updated successfully: {VideoId}", video.VideoId);

                return Json(new
                {
                    success = true,
                    message = "Video updated successfully!",
                    video = new
                    {
                        videoId = video.VideoId,
                        title = video.Title,
                        videoNumber = video.VideoNumber,
                       
                        
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Video: {VideoId}", video?.VideoId);
                return Json(new { success = false, message = "An error occurred while updating the Video: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to delete a product.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get Video details first to clean up image
                var product = await _videoApi.GetVideoByIdAsync(id, accessToken, cancellationToken);
                

                    // Delete product
                    var result = await _videoApi.DeleteVideoFileAsync(id, accessToken, cancellationToken);

                if (result.Success)
                {
                    var uploadResult = await _storageApi.RemoveVideoFileAsync($"{product.VideoId}.webp");
                    
                }
                else
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Product");
                }


                TempData["Success"] = "Product deleted successfully.";
                return RedirectToAction("Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Product: {ProductId}", id);
                TempData["Error"] = "An error occurred while deleting the Product. Please try again later.";
                return RedirectToAction("Product");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateImageUrl([FromBody] ValidateVideoUrlRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.VideoUrl))
                {
                    return Json(new { success = false, message = "No URL provided" });
                }

                var validation = await ValidateVideoUrl(request.VideoUrl);

                if (validation.IsValid)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Valid image URL",
                        VideoUrl = request.VideoUrl
                    });
                }
                else
                {
                    return Json(new { success = false, message = validation.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image URL: {VideoUrl}", request?.VideoUrl);
                return Json(new { success = false, message = "Error validating image URL" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile imageFile, string productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    return Json(new { success = false, message = "No image file provided" });
                }

                var validationResult = ValidateImageFile(imageFile);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var uploadResult = await _storageApi.UpdateProductImageFileAsync(productId, imageFile);
                if (!uploadResult)
                {
                    return Json(new { success = false, message = "Failed to upload image" });
                }

                var imageUrl = $"/api/storage/product/{productId}{Path.GetExtension(imageFile.FileName).ToLower()}";

                // Update product with new image URL if productId is provided
                if (!string.IsNullOrEmpty(productId))
                {
                    try
                    {
                        var product = await _videoApi.GetVideoByIdAsync(productId, accessToken, cancellationToken);
                        if (product != null)
                        {
                            product.VideoURL = imageUrl;
                            await _videoApi.UpdateVideoFileAsync(product, accessToken, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating Video image URL for product: {VideoId}", productId);
                        // Continue anyway - the file was uploaded successfully
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Image uploaded successfully",
                    imageUrl = imageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading Video video");
                return Json(new { success = false, message = "Failed to upload image. Please try again." });
            }
        }

        // ========== VALIDATION METHODS ==========

        private ValidationResult ValidateVideo(Video video)
        {
            if (string.IsNullOrWhiteSpace(video.Title))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Product title is required", Field = "Title" };
            }

            if (video.Title.Length > 100)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Product title cannot exceed 100 characters", Field = "Title" };
            }

          
            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateImageFile(IFormFile file)
        {
            const int maxFileSize = 5 * 1024 * 1024; // 5MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp" };

            if (file.Length > maxFileSize)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size ({FormatFileSize(file.Length)}) exceeds maximum allowed size (5MB)"
                };
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid file type. Allowed types: JPG, PNG, GIF, WebP, BMP"
                };
            }

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid file content type. Please upload a valid image file."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        private async Task<ValidationResult> ValidateVideoUrl(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Image URL is required" };
            }

            if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out Uri uri))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Invalid URL format" };
            }

            // Check if URL points to an image based on extension
            var path = uri.AbsolutePath.ToLowerInvariant();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

            if (!imageExtensions.Any(ext => path.EndsWith(ext)))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "URL does not appear to point to a valid image file" };
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Image URL is not accessible" };
                }

                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(contentType) && !contentType.StartsWith("image/"))
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "URL does not point to an image" };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating image URL: {ImageUrl}", videoUrl);
                return new ValidationResult { IsValid = false, ErrorMessage = "Could not validate image URL. Please check the URL and try again." };
            }
        }

        private string FormatFileSize(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "B", "KB", "MB", "GB" };

            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:0.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }

            return "0 B";
        }

        // ========== HELPER CLASSES ==========

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
            public string Field { get; set; }
        }
    }

    public class ValidateVideoUrlRequest
    {
        public string VideoUrl { get; set; }
    }
}