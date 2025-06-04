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
        public async Task<IActionResult> Video(string cursor = null, int limit = 10, string direction = "next", string sortBy = "VideoDate", CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view videos.";
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
                _logger.LogError(ex, "Error retrieving videos");
                TempData["Error"] = "An error occurred while retrieving videos. Please try again later.";
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
                        videoName = video.VideoName,
                        videoURL = video.VideoURL,
                        videoThumbnail = video.VideoThumbnail,
                        status = video.Status,
                        videoDate = video.VideoDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        createdDate = video.CreatedDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        clientId = video.ClientId,
                        videoNumber = video.VideoNumber
                    }
                };

                return Json(videoData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video data for ID: {videoId}", id);
                return Json(new { success = false, message = "Error loading video data: " + ex.Message });
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
                    TempData["Error"] = "You must be logged in to view video details.";
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
                _logger.LogError(ex, "Error retrieving video details for ID: {VideoId}", id);
                TempData["Error"] = "An error occurred while retrieving video details. Please try again later.";
                return RedirectToAction("Video");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a video.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Video());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(838_860_800)] // 800MB limit (800 * 1024 * 1024)
        [RequestFormLimits(MultipartBodyLengthLimit = 838_860_800)]
        public async Task<IActionResult> Create([FromForm] Video video, IFormFile VideoFile, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Create Video - Starting upload process");

                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Create Video - No access token found");
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                // Enhanced debugging
                _logger.LogInformation("Create Video - Received data. Video: {@Video}, VideoFile: {VideoFile}",
                    video, VideoFile?.FileName);

                // If video is null, try to create from form data manually
                if (video == null)
                {
                    _logger.LogWarning("Create Video - Video object is null, attempting to create from form");
                    video = new Video();

                    // Try to get values from form collection
                    var form = Request.Form;
                    video.Title = form["Title"].FirstOrDefault() ?? form["VideoName"].FirstOrDefault();
                    video.VideoName = form["VideoName"].FirstOrDefault() ?? form["Title"].FirstOrDefault();
                    video.Status = form["Status"].FirstOrDefault() ?? "Active";
                    video.ClientId = form["ClientId"].FirstOrDefault();

                    // Try to parse VideoDate if provided
                    if (DateTime.TryParse(form["VideoDate"].FirstOrDefault(), out DateTime videoDate))
                    {
                        video.VideoDate = videoDate;
                    }

                    _logger.LogInformation("Create Video - Manually created video from form: {@Video}", video);
                }

                // Enhanced validation with better error messages
                var validationResult = ValidateVideoEnhanced(video, VideoFile);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Create Video - Validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                    return Json(new
                    {
                        success = false,
                        message = validationResult.ErrorMessage,
                        field = validationResult.Field
                    });
                }

                // Set default values
                video.VideoId = Guid.NewGuid().ToString();
                video.VideoNumber = UniqueIdNumber.GenerateSixDigit();
                video.Status = video.Status ?? "Active";
                video.CreatedDate = DateTime.UtcNow;
                video.VideoDate = video.VideoDate ?? DateTime.UtcNow;

                _logger.LogInformation("Create Video - Processing video upload for VideoId: {VideoId}", video.VideoId);

                // Handle video file upload
                string videoUrl = null;
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    try
                    {
                        _logger.LogInformation("Create Video - Uploading file: {FileName}, Size: {Size}, ContentType: {ContentType}",
                            VideoFile.FileName, VideoFile.Length, VideoFile.ContentType);

                        var fileName = video.VideoId + Path.GetExtension(VideoFile.FileName).ToLower();
                        var uploadResult = await _storageApi.UpdateVideoFileAsync(video.VideoId, VideoFile);

                        if (uploadResult)
                        {
                            videoUrl = $"/api/storage/video/{fileName}";
                            video.VideoURL = videoUrl;
                            _logger.LogInformation("Create Video - File uploaded successfully: {VideoUrl}", videoUrl);
                        }
                        else
                        {
                            _logger.LogError("Create Video - File upload failed for VideoId: {VideoId}", video.VideoId);
                            return Json(new { success = false, message = "Failed to upload video file. Please try again." });
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Create Video - Error uploading video file for VideoId: {VideoId}", video.VideoId);
                        return Json(new
                        {
                            success = false,
                            message = $"Error uploading video file: {uploadEx.Message}. Please check file size and format."
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(video.VideoURL))
                {
                    // Validate video URL if provided
                    var urlValidation = await ValidateVideoUrl(video.VideoURL);
                    if (!urlValidation.IsValid)
                    {
                        _logger.LogWarning("Create Video - Invalid video URL: {VideoUrl}", video.VideoURL);
                        return Json(new { success = false, message = urlValidation.ErrorMessage });
                    }
                    videoUrl = video.VideoURL;
                }
                else
                {
                    _logger.LogWarning("Create Video - No video file or URL provided");
                    return Json(new { success = false, message = "Please provide either a video file or a valid video URL." });
                }

                // Create Video
                _logger.LogInformation("Create Video - Calling API to create video");
                var createdVideo = await _videoApi.CreateVideoAsync(video, accessToken, cancellationToken);

                if (createdVideo != null)
                {
                    _logger.LogInformation("Create Video - Video created successfully: {VideoId}", video.VideoId);

                    return Json(new
                    {
                        success = true,
                        message = "Video uploaded and created successfully!",
                        video = new
                        {
                            videoId = video.VideoId,
                            title = video.Title,
                            videoNumber = video.VideoNumber,
                            clientId = video.ClientId,
                            videoName = video.VideoName,
                            videoURL = video.VideoURL,
                            videoDate = video.VideoDate?.ToString("yyyy-MM-dd"),
                            status = video.Status,
                        }
                    });
                }
                else
                {
                    _logger.LogError("Create Video - API returned null for VideoId: {VideoId}", video.VideoId);
                    return Json(new { success = false, message = "Failed to create video record. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Video - Unexpected error occurred");
                return Json(new
                {
                    success = false,
                    message = $"An unexpected error occurred: {ex.Message}. Please try again or contact support."
                });
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
                    TempData["Error"] = "You must be logged in to edit a video.";
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
                _logger.LogError(ex, "Error retrieving video for edit, ID: {VideoId}", id);
                TempData["Error"] = "An error occurred while retrieving the video. Please try again later.";
                return RedirectToAction("Video");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(838_860_800)] // 800MB limit for edit as well
        [RequestFormLimits(MultipartBodyLengthLimit = 838_860_800)]
        public async Task<IActionResult> Edit(Video video, IFormFile VideoFile, bool RemoveVideo = false, CancellationToken cancellationToken = default)
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
                var validationResult = ValidateVideoEnhanced(video, VideoFile, isEdit: true);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage, field = validationResult.Field });
                }

                // Get existing video
                var existingVideo = await _videoApi.GetVideoByIdAsync(video.VideoId, accessToken, cancellationToken);
                if (existingVideo == null)
                {
                    return Json(new { success = false, message = "Video not found" });
                }

                // Handle video operations
                string videoUrl = existingVideo.VideoURL;

                if (RemoveVideo)
                {
                    // Remove video
                    videoUrl = null;
                    video.VideoURL = null;
                }
                else if (VideoFile != null && VideoFile.Length > 0)
                {
                    // Validate and upload new video
                    var fileValidation = ValidateVideoFile(VideoFile);
                    if (!fileValidation.IsValid)
                    {
                        return Json(new { success = false, message = fileValidation.ErrorMessage });
                    }

                    try
                    {
                        var uploadResult = await _storageApi.UpdateVideoFileAsync(video.VideoId, VideoFile);
                        if (uploadResult)
                        {
                            videoUrl = $"/api/storage/video/{video.VideoId}{Path.GetExtension(VideoFile.FileName).ToLower()}";
                            video.VideoURL = videoUrl;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to upload video. Please try again." });
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading video for Video: {VideoId}", video.VideoId);
                        return Json(new { success = false, message = "Error uploading video: " + uploadEx.Message });
                    }
                }
                else if (!string.IsNullOrEmpty(video.VideoURL) && video.VideoURL != existingVideo.VideoURL)
                {
                    // Validate new video URL
                    var urlValidation = await ValidateVideoUrl(video.VideoURL);
                    if (!urlValidation.IsValid)
                    {
                        return Json(new { success = false, message = urlValidation.ErrorMessage });
                    }
                    videoUrl = video.VideoURL;
                }
                else
                {
                    // Keep existing video
                    video.VideoURL = existingVideo.VideoURL;
                }

                // Set default values
                video.Status = video.Status ?? "Active";
                video.VideoNumber = existingVideo.VideoNumber; // Preserve original video number

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
                        videoName = video.VideoName,
                        status = video.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating video: {VideoId}", video?.VideoId);
                return Json(new { success = false, message = "An error occurred while updating the video: " + ex.Message });
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
                    TempData["Error"] = "You must be logged in to delete a video.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get video details first to clean up files
                var video = await _videoApi.GetVideoByIdAsync(id, accessToken, cancellationToken);

                if (video != null)
                {
                    // Delete video
                    var result = await _videoApi.DeleteVideoFileAsync(id, accessToken, cancellationToken);

                    if (result.Success)
                    {
                        // Remove video file from storage
                        var uploadResult = await _storageApi.RemoveVideoFileAsync($"{video.VideoId}.mp4");
                        // Also try to remove other common video formats
                        await _storageApi.RemoveVideoFileAsync($"{video.VideoId}.webm");
                        await _storageApi.RemoveVideoFileAsync($"{video.VideoId}.ogg");
                    }
                    else
                    {
                        TempData["Error"] = result.ErrorMessage ?? "Failed to delete video.";
                        return RedirectToAction("Video");
                    }
                }
                else
                {
                    TempData["Error"] = "Video not found.";
                    return RedirectToAction("Video");
                }

                TempData["Success"] = "Video deleted successfully.";
                return RedirectToAction("Video");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video: {VideoId}", id);
                TempData["Error"] = "An error occurred while deleting the video. Please try again later.";
                return RedirectToAction("Video");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateVideoUrl([FromBody] ValidateVideoUrlRequest request, CancellationToken cancellationToken = default)
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
                        message = "Valid video URL",
                        videoUrl = request.VideoUrl
                    });
                }
                else
                {
                    return Json(new { success = false, message = validation.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating video URL: {VideoUrl}", request?.VideoUrl);
                return Json(new { success = false, message = "Error validating video URL" });
            }
        }

        // ========== ENHANCED VALIDATION METHODS ==========

        // Enhanced validation method with 800MB support
        private ValidationResult ValidateVideoEnhanced(Video video, IFormFile videoFile, bool isEdit = false)
        {
            _logger.LogInformation("Validating video - Title: {Title}, HasFile: {HasFile}",
                video?.Title, videoFile != null && videoFile.Length > 0);

            // Check if we have either Title or VideoName
            var hasTitle = !string.IsNullOrWhiteSpace(video?.Title);
            var hasVideoName = !string.IsNullOrWhiteSpace(video?.VideoName);

            if (!hasTitle && !hasVideoName)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Video title is required. Please enter a title for your video.",
                    Field = "Title"
                };
            }

            var titleToCheck = hasTitle ? video.Title : video.VideoName;

            if (titleToCheck.Length > 100)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Video title cannot exceed 100 characters. Please use a shorter title.",
                    Field = "Title"
                };
            }

            // Check if we have a video source (file or URL)
            var hasVideoFile = videoFile != null && videoFile.Length > 0;
            var hasVideoUrl = !string.IsNullOrWhiteSpace(video?.VideoURL);

            // For new videos, require either file or URL
            if (!isEdit && !hasVideoFile && !hasVideoUrl)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Please provide either a video file or a video URL.",
                    Field = "VideoFile"
                };
            }

            // Validate video file if provided
            if (hasVideoFile)
            {
                var fileValidation = ValidateVideoFile(videoFile);
                if (!fileValidation.IsValid)
                {
                    return fileValidation;
                }
            }

            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateVideoFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ValidationResult { IsValid = true }; // Optional file
            }

            const long maxFileSize = 838_860_800; // 800MB in bytes (800 * 1024 * 1024)
            var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
            var allowedContentTypes = new[] {
                "video/mp4", "video/webm", "video/ogg", "video/avi",
                "video/quicktime", "video/x-msvideo", "video/x-flv", "video/x-matroska",
                "application/octet-stream" // Some browsers use this for video files
            };

            _logger.LogInformation("Validating video file - Name: {FileName}, Size: {Size}, ContentType: {ContentType}",
                file.FileName, file.Length, file.ContentType);

            if (file.Length > maxFileSize)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Video file size ({FormatFileSize(file.Length)}) exceeds the maximum allowed size of 800MB. Please compress your video or use a smaller file."
                };
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Video file type '{extension}' is not supported. Please use one of these formats: {string.Join(", ", allowedExtensions)}"
                };
            }

            // More lenient content type checking since browsers can be inconsistent
            if (!string.IsNullOrEmpty(file.ContentType) &&
                !allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()) &&
                !file.ContentType.StartsWith("video/"))
            {
                _logger.LogWarning("Unexpected content type for video file: {ContentType}", file.ContentType);
                // Don't fail validation, just log a warning
            }

            return new ValidationResult { IsValid = true };
        }

        private async Task<ValidationResult> ValidateVideoUrl(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Video URL cannot be empty" };
            }

            if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out Uri uri))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Please enter a valid URL format (e.g., https://example.com/video.mp4)" };
            }

            // Check if URL points to a video based on extension
            var path = uri.AbsolutePath.ToLowerInvariant();
            var videoExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv", ".flv", ".mkv" };

            if (!videoExtensions.Any(ext => path.EndsWith(ext)))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "URL does not appear to point to a supported video file. Please ensure the URL ends with a video file extension."
                };
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Video URL is not accessible (HTTP {(int)response.StatusCode}). Please check the URL and try again."
                    };
                }

                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(contentType) && !contentType.StartsWith("video/"))
                {
                    _logger.LogWarning("URL does not return video content type: {ContentType}", contentType);
                    // Don't fail validation, just warn
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating video URL: {VideoUrl}", videoUrl);
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Could not validate video URL. Please check that the URL is correct and accessible."
                };
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