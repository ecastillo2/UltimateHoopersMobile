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
        public async Task<IActionResult> Create([FromForm] Video video, IFormFile VideoFile, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                // Debug logging to see what's being received
                _logger.LogInformation("Create Video - Received video object: {@Video}", video);
                _logger.LogInformation("Create Video - VideoFile: {FileName}, Size: {Size}",
                    VideoFile?.FileName, VideoFile?.Length);

                // Log all form keys to see what's actually being sent
                _logger.LogInformation("Form keys: {Keys}", string.Join(", ", Request.Form.Keys));
                _logger.LogInformation("File keys: {FileKeys}", string.Join(", ", Request.Form.Files.Select(f => f.Name)));

                // If VideoFile is null, try to get it from different possible names
                if (VideoFile == null && Request.Form.Files.Any())
                {
                    // Try common file input names
                    VideoFile = Request.Form.Files["VideoFile"] ??
                               Request.Form.Files["videoFile"] ??
                               Request.Form.Files["ImageFile"] ?? // In case the view is still using ImageFile
                               Request.Form.Files["imageFile"] ??
                               Request.Form.Files.FirstOrDefault();

                    if (VideoFile != null)
                    {
                        _logger.LogInformation("Found video file with name: {Name}, FileName: {FileName}",
                            VideoFile.Name, VideoFile.FileName);
                    }
                }

                // If video is null, try to create from form data manually
                if (video == null)
                {
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

                // Ensure we have required fields
                if (string.IsNullOrWhiteSpace(video.Title) && string.IsNullOrWhiteSpace(video.VideoName))
                {
                    return Json(new { success = false, message = "Video title or name is required", field = "Title" });
                }

                // Use VideoName as Title if Title is empty, or vice versa
                if (string.IsNullOrWhiteSpace(video.Title))
                    video.Title = video.VideoName;
                if (string.IsNullOrWhiteSpace(video.VideoName))
                    video.VideoName = video.Title;

                // Enhanced validation
                var validationResult = ValidateVideo(video);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage, field = validationResult.Field });
                }

                // Validate video file if provided
                if (VideoFile != null)
                {
                    _logger.LogInformation("Validating video file: {FileName}, Size: {Size}, ContentType: {ContentType}",
                        VideoFile.FileName, VideoFile.Length, VideoFile.ContentType);

                    var fileValidation = ValidateVideoFile(VideoFile);
                    if (!fileValidation.IsValid)
                    {
                        return Json(new { success = false, message = fileValidation.ErrorMessage });
                    }
                }
                else
                {
                    _logger.LogWarning("No video file received in request");
                }

                // Set default values
                video.VideoId = Guid.NewGuid().ToString();
                video.VideoNumber = UniqueIdNumber.GenerateSixDigit();
                video.Status = video.Status ?? "Active";
                video.CreatedDate = DateTime.UtcNow;
                video.VideoDate = video.VideoDate ?? DateTime.UtcNow;

                // Handle video file upload
                string videoUrl = null;
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    try
                    {
                        var fileName = video.VideoId + Path.GetExtension(VideoFile.FileName).ToLower();
                        var uploadResult = await _storageApi.UpdateVideoFileAsync(video.VideoId, VideoFile);

                        if (uploadResult)
                        {
                            videoUrl = $"/api/storage/video/{fileName}";
                            video.VideoURL = videoUrl;
                            _logger.LogInformation("Video uploaded successfully: {VideoUrl}", videoUrl);
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
                else if (!string.IsNullOrEmpty(video.VideoURL))
                {
                    // Validate video URL if provided
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
                            clientId = video.ClientId,
                            videoName = video.VideoName,
                            videoURL = video.VideoURL,
                            videoDate = video.VideoDate,
                            status = video.Status,
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create video. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video");
                return Json(new { success = false, message = "An error occurred while creating the video: " + ex.Message });
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
                var validationResult = ValidateVideo(video);
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

        // ========== VALIDATION METHODS ==========

        private ValidationResult ValidateVideo(Video video)
        {
            // Check if we have either Title or VideoName
            var hasTitle = !string.IsNullOrWhiteSpace(video.Title);
            var hasVideoName = !string.IsNullOrWhiteSpace(video.VideoName);

            if (!hasTitle && !hasVideoName)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Video title or name is required", Field = "Title" };
            }

            var titleToCheck = hasTitle ? video.Title : video.VideoName;

            if (titleToCheck.Length > 100)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Video title cannot exceed 100 characters", Field = "Title" };
            }

            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateVideoFile(IFormFile file)
        {
            const int maxFileSize = 100 * 1024 * 1024; // 100MB for videos
            var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
            var allowedContentTypes = new[] {
                "video/mp4", "video/webm", "video/ogg", "video/avi",
                "video/quicktime", "video/x-msvideo", "video/x-flv", "video/x-matroska"
            };

            if (file.Length > maxFileSize)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size ({FormatFileSize(file.Length)}) exceeds maximum allowed size (100MB)"
                };
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid file type. Allowed types: MP4, WebM, OGG, AVI, MOV, WMV, FLV, MKV"
                };
            }

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid file content type. Please upload a valid video file."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        private async Task<ValidationResult> ValidateVideoUrl(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Video URL is required" };
            }

            if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out Uri uri))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Invalid URL format" };
            }

            // Check if URL points to a video based on extension
            var path = uri.AbsolutePath.ToLowerInvariant();
            var videoExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv", ".flv", ".mkv" };

            if (!videoExtensions.Any(ext => path.EndsWith(ext)))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "URL does not appear to point to a valid video file" };
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "Video URL is not accessible" };
                }

                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(contentType) && !contentType.StartsWith("video/"))
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "URL does not point to a video" };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating video URL: {VideoUrl}", videoUrl);
                return new ValidationResult { IsValid = false, ErrorMessage = "Could not validate video URL. Please check the URL and try again." };
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