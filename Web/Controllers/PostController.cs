using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Web;
using WebAPI.ApiClients;
using Website.ViewModels;

namespace Website.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostApi _postApi;
        private readonly ILogger<PostController> _logger;

        public PostController(IPostApi postApi, ILogger<PostController> logger)
        {
            _postApi = postApi ?? throw new ArgumentNullException(nameof(postApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Post(string postType, string cursor = null, int limit = 10, string direction = "next", string sortBy = "StartDate", CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view posts.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get current user role and profile ID
                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                // Get posts with cursor pagination
                var result = await _postApi.GetPostsAsync(postType, accessToken, cancellationToken);

                // Create view model
                var viewModel = new PostsViewModel
                {
                    PostList = result,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                TempData["Error"] = "An error occurred while retrieving posts. Please try again later.";
                return RedirectToAction("Dashboard", "Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPostData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Get post details
                var post = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (post == null)
                {
                    return Json(new { success = false, message = "Post not found" });
                }

                // Format the response with comprehensive post data
                var postData = new
                {
                    success = true,
                    post = new
                    {
                        postId = post.PostId,
                        title = post.Title,
                        caption = post.Caption,
                        content = post.PostText, // Rich text content
                        description = post.Caption, // Use caption as description
                        postType = post.PostType,
                        status = post.Status,
                        imageURL = post.PostFileURL,
                        thumbnailURL = post.ThumbnailUrl,
                        postedDate = post.PostedDate?.ToString("yyyy-MM-ddTHH:mm") ?? "",
                        profileId = post.ProfileId,
                        author = post.ProfileId ?? "System",
                        isActive = post.Status?.ToLower() == "active",
                        createdDate = post.PostedDate?.ToString("yyyy-MM-ddTHH:mm") ?? "",
                        views = GetRandomViews(),
                        likes = GetRandomLikes(),
                        comments = GetRandomComments(),
                        shares = GetRandomShares(),
                        // Additional content info for rich text
                        contentType = "html",
                        hasRichContent = HasRichTextContent(post.PostText),
                        plainTextPreview = GetPlainTextPreview(post.PostText, 150)
                    },
                    // Additional metadata
                    metadata = new
                    {
                        hasImage = !string.IsNullOrEmpty(post.PostFileURL),
                        hasContent = !string.IsNullOrEmpty(post.PostText),
                        hasDescription = !string.IsNullOrEmpty(post.Caption),
                        contentLength = post.PostText?.Length ?? 0,
                        plainTextLength = GetPlainTextFromHtml(post.PostText ?? "").Length
                    }
                };

                return Json(postData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post data for ID: {PostId}", id);
                return Json(new { success = false, message = "Error retrieving post data", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view post details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get post details
                var post = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Post");
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post details for ID: {PostId}", id);
                TempData["Error"] = "An error occurred while retrieving post details. Please try again later.";
                return RedirectToAction("Post");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a post.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Post());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile ImageFile, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a post.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Set creator profile ID
                post.ProfileId = HttpContext.Session.GetString("ProfileId");

                // Process rich text content
                post = ProcessRichTextContent(post);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(post.Title))
                {
                    TempData["Error"] = "Post title is required.";
                    return View(post);
                }

                if (string.IsNullOrWhiteSpace(GetPlainTextFromHtml(post.PostText ?? "")))
                {
                    TempData["Error"] = "Post content is required.";
                    return View(post);
                }

                // Set default values
                post.Status = post.Status ?? "Active";
                post.PostedDate = post.PostedDate ?? DateTime.UtcNow;

                // Handle image upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var imageResult = await ProcessImageUpload(ImageFile);
                    if (imageResult.Success)
                    {
                        post.PostFileURL = imageResult.ImageUrl;
                        post.ThumbnailUrl = imageResult.ImageUrl; // For now, use same URL
                    }
                    else
                    {
                        TempData["Error"] = imageResult.ErrorMessage;
                        return View(post);
                    }
                }

                // Create new post
                var createdPost = await _postApi.CreatePostAsync(post, accessToken, cancellationToken);

                TempData["Success"] = "Post created successfully.";
                return RedirectToAction("Details", new { id = createdPost.PostId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                TempData["Error"] = "An error occurred while creating the post. Please try again later.";
                return View(post);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a post.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get post details
                var post = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Post");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (post.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to edit this post.";
                    return RedirectToAction("Details", new { id = id });
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post for edit, ID: {PostId}", id);
                TempData["Error"] = "An error occurred while retrieving the post. Please try again later.";
                return RedirectToAction("Post");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Post post, IFormFile ImageFile, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Get user info for permission checking
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Get the existing post to check permissions
                var existingPost = await _postApi.GetPostByIdAsync(post.PostId, accessToken, cancellationToken);
                if (existingPost == null)
                {
                    return Json(new { success = false, message = "Post not found" });
                }

                // Verify user has permission to edit this post
                if (existingPost.ProfileId != profileId && userRole != "Admin")
                {
                    return Json(new { success = false, message = "You do not have permission to edit this post" });
                }

                // Process rich text content
                post = ProcessRichTextContent(post);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(post.Title))
                {
                    return Json(new { success = false, message = "Post title is required" });
                }

                if (string.IsNullOrWhiteSpace(GetPlainTextFromHtml(post.PostText ?? "")))
                {
                    return Json(new { success = false, message = "Post content is required" });
                }

                // Handle image upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var imageResult = await ProcessImageUpload(ImageFile);
                    if (imageResult.Success)
                    {
                        post.PostFileURL = imageResult.ImageUrl;
                        post.ThumbnailUrl = imageResult.ImageUrl;
                    }
                    else
                    {
                        return Json(new { success = false, message = imageResult.ErrorMessage });
                    }
                }
                else
                {
                    // Keep existing image URLs if no new image
                    post.PostFileURL = existingPost.PostFileURL;
                    post.ThumbnailUrl = existingPost.ThumbnailUrl;
                }

                // Preserve original metadata
                post.ProfileId = existingPost.ProfileId;
                post.PostedDate = existingPost.PostedDate;

                // Update post via API
                await _postApi.UpdatePostAsync(post, accessToken, cancellationToken);

                _logger.LogInformation("Post updated successfully by user {ProfileId}: {PostId}", profileId, post.PostId);

                return Json(new
                {
                    success = true,
                    message = "Post updated successfully!",
                    postId = post.PostId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post: {PostId}", post?.PostId);
                return Json(new { success = false, message = "An unexpected error occurred while updating the post" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to delete a post.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get post details first to check permissions
                var post = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Post");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (post.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to delete this post.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Delete post
                await _postApi.DeletePostAsync(id, accessToken, cancellationToken);

                TempData["Success"] = "Post deleted successfully.";
                return RedirectToAction("Post");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post: {PostId}", id);
                TempData["Error"] = "An error occurred while deleting the post. Please try again later.";
                return RedirectToAction("Post");
            }
        }

        #region Rich Text Content Processing

        /// <summary>
        /// Process rich text content from TinyMCE editor
        /// </summary>
        /// <param name="post">The post object to process</param>
        /// <returns>Post with processed content</returns>
        private Post ProcessRichTextContent(Post post)
        {
            if (string.IsNullOrEmpty(post.PostText))
            {
                return post;
            }

            try
            {
                // Clean up the HTML content
                var cleanedContent = CleanHtmlContent(post.PostText);

                // Store the cleaned HTML content
                post.PostText = cleanedContent;

                // Generate plain text preview for search/indexing
                var plainText = GetPlainTextFromHtml(cleanedContent);

                // If caption is empty, generate it from content
                if (string.IsNullOrEmpty(post.Caption) && !string.IsNullOrEmpty(plainText))
                {
                    post.Caption = GetPlainTextPreview(plainText, 200);
                }

                _logger.LogInformation("Processed rich text content for post. Original length: {OriginalLength}, Cleaned length: {CleanedLength}",
                    post.PostText.Length, cleanedContent.Length);

                return post;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rich text content");
                return post;
            }
        }

        /// <summary>
        /// Clean HTML content to remove potentially harmful or unnecessary elements
        /// </summary>
        /// <param name="htmlContent">Raw HTML content from editor</param>
        /// <returns>Cleaned HTML content</returns>
        private string CleanHtmlContent(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return string.Empty;

            // Remove script tags and their content for security
            htmlContent = Regex.Replace(htmlContent, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove potentially harmful attributes
            htmlContent = Regex.Replace(htmlContent, @"on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);

            // Remove empty paragraphs that TinyMCE sometimes creates
            htmlContent = Regex.Replace(htmlContent, @"<p[^>]*>\s*(&nbsp;)?\s*</p>", "", RegexOptions.IgnoreCase);

            // Clean up excessive whitespace
            htmlContent = Regex.Replace(htmlContent, @"\s+", " ");

            // Trim whitespace
            htmlContent = htmlContent.Trim();

            return htmlContent;
        }

        /// <summary>
        /// Extract plain text from HTML content
        /// </summary>
        /// <param name="htmlContent">HTML content</param>
        /// <returns>Plain text content</returns>
        private string GetPlainTextFromHtml(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return string.Empty;

            try
            {
                // Decode HTML entities
                string decoded = HttpUtility.HtmlDecode(htmlContent);

                // Remove HTML tags
                string plainText = Regex.Replace(decoded, @"<[^>]+>", "");

                // Clean up whitespace
                plainText = Regex.Replace(plainText, @"\s+", " ");

                return plainText.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting plain text from HTML");
                return htmlContent; // Return original if processing fails
            }
        }

        /// <summary>
        /// Get a preview of plain text content
        /// </summary>
        /// <param name="text">Plain text content</param>
        /// <param name="maxLength">Maximum length of preview</param>
        /// <returns>Text preview</returns>
        private string GetPlainTextPreview(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length <= maxLength)
                return text;

            // Find the last space before the max length to avoid cutting words
            int lastSpace = text.LastIndexOf(' ', maxLength);
            if (lastSpace > 0 && lastSpace > maxLength - 20) // Only use if it's reasonably close to maxLength
            {
                return text.Substring(0, lastSpace) + "...";
            }

            return text.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Check if content contains rich text formatting
        /// </summary>
        /// <param name="content">Content to check</param>
        /// <returns>True if content has rich text formatting</returns>
        private bool HasRichTextContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            // Check for common HTML tags that indicate rich formatting
            var richTextIndicators = new[]
            {
                "<strong>", "<b>", "<em>", "<i>", "<u>",
                "<h1>", "<h2>", "<h3>", "<h4>", "<h5>", "<h6>",
                "<ul>", "<ol>", "<li>", "<blockquote>",
                "<a ", "<img ", "<table>", "<div", "<span"
            };

            return richTextIndicators.Any(indicator =>
                content.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Image Processing

        /// <summary>
        /// Process image file upload
        /// </summary>
        private async Task<(bool Success, string ImageUrl, string ErrorMessage)> ProcessImageUpload(IFormFile imageFile)
        {
            try
            {
                // Validate file
                var validationResult = ValidateImageFile(imageFile);
                if (!validationResult.IsValid)
                {
                    return (false, null, validationResult.ErrorMessage);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Define upload path
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "posts");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Generate URL
                var imageUrl = $"/uploads/posts/{fileName}";

                _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);
                return (true, imageUrl, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image file");
                return (false, null, "Failed to upload image. Please try again.");
            }
        }

        /// <summary>
        /// Validate uploaded image file
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "No file provided");
            }

            // Check file size (5MB limit)
            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, "File size must be less than 5MB");
            }

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, "Only image files (JPG, PNG, GIF, BMP, WEBP) are allowed");
            }

            // Check content type
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid file type");
            }

            return (true, null);
        }

        #endregion

        // Helper methods for mock data (replace with real data when available)
        private int GetRandomViews() => new Random().Next(50, 1000);
        private int GetRandomLikes() => new Random().Next(5, 100);
        private int GetRandomComments() => new Random().Next(0, 50);
        private int GetRandomShares() => new Random().Next(0, 25);
    }
}