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

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Post ID is required" });
                }

                // Get post details
                var post = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (post == null)
                {
                    return Json(new { success = false, message = "Post not found" });
                }

                // Determine media type from file URL
                var mediaType = DetermineMediaType(post.PostFileURL);
                var hasMedia = !string.IsNullOrEmpty(post.PostFileURL);

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
                        postText = post.PostText, // Also include as postText for compatibility
                        postType = post.PostType,
                        type = post.Type, // Include both for compatibility
                        status = post.Status,

                        // Enhanced media information
                        imageURL = post.PostFileURL,
                        imageUrl = post.PostFileURL, // Include both cases
                        postFileURL = post.PostFileURL,
                        PostFileURL = post.PostFileURL,
                        thumbnailURL = post.ThumbnailUrl,
                        thumbnailUrl = post.ThumbnailUrl, // Include both cases

                        // Media metadata
                        mediaType = mediaType,
                        hasMedia = hasMedia,
                        isVideo = mediaType == "video",
                        isImage = mediaType == "image",

                        postedDate = post.PostedDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                        profileId = post.ProfileId,
                        author = post.ProfileId ?? "System",
                        isActive = post.Status?.ToLower() == "active",
                        createdDate = post.PostedDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
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
                        plainTextLength = GetPlainTextFromHtml(post.PostText ?? "").Length,
                        mediaType = mediaType,
                        mediaUrl = post.PostFileURL,
                        thumbnailUrl = post.ThumbnailUrl
                    }
                };

                _logger.LogInformation("Successfully retrieved post data for ID: {PostId}", id);
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

                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Invalid post ID.";
                    return RedirectToAction("Post");
                }

                // Get post details
                var post = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Post");
                }

                // Increment view count (if you have this functionality)
                try
                {
                    // You can add view tracking here if needed
                    _logger.LogInformation("Post {PostId} viewed by user {UserId}", id, HttpContext.Session.GetString("ProfileId"));
                }
                catch (Exception viewEx)
                {
                    _logger.LogWarning(viewEx, "Failed to increment view count for post {PostId}", id);
                    // Continue anyway - this shouldn't prevent showing the post
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
        public async Task<IActionResult> Create(Post post, CancellationToken cancellationToken = default)
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

                if (string.IsNullOrWhiteSpace(post.Caption))
                {
                    TempData["Error"] = "Post caption is required.";
                    return View(post);
                }

                // Set default values
                post.Status = post.Status ?? "Active";
                post.PostedDate = post.PostedDate ?? DateTime.UtcNow;

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
        public async Task<IActionResult> Edit(Post post, CancellationToken cancellationToken = default)
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

                // Process rich text content if it's from PostText field
                if (!string.IsNullOrEmpty(post.PostText))
                {
                    post.Caption = post.PostText; // Use PostText as Caption if provided
                }
                post = ProcessRichTextContent(post);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(post.Title))
                {
                    return Json(new { success = false, message = "Post title is required" });
                }

                if (string.IsNullOrWhiteSpace(post.Caption))
                {
                    return Json(new { success = false, message = "Post caption is required" });
                }

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
            if (string.IsNullOrEmpty(post.Caption))
            {
                return post;
            }

            try
            {
                // Clean up the HTML content
                var cleanedContent = CleanHtmlContent(post.Caption);

                // Store the cleaned HTML content
                post.Caption = cleanedContent;
                post.PostText = cleanedContent; // Also set PostText for consistency

                // Generate plain text preview for search/indexing
                var plainText = GetPlainTextFromHtml(cleanedContent);

                _logger.LogInformation("Processed rich text content for post. Original length: {OriginalLength}, Cleaned length: {CleanedLength}",
                    post.Caption?.Length ?? 0, cleanedContent.Length);

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

            // First convert HTML to plain text
            var plainText = GetPlainTextFromHtml(text);

            if (plainText.Length <= maxLength)
                return plainText;

            // Find the last space before the max length to avoid cutting words
            int lastSpace = plainText.LastIndexOf(' ', maxLength);
            if (lastSpace > 0 && lastSpace > maxLength - 20) // Only use if it's reasonably close to maxLength
            {
                return plainText.Substring(0, lastSpace) + "...";
            }

            return plainText.Substring(0, maxLength) + "...";
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

        /// <summary>
        /// Determine media type from file URL
        /// </summary>
        /// <param name="fileUrl">File URL to analyze</param>
        /// <returns>Media type (image, video, or unknown)</returns>
        private string DetermineMediaType(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return "unknown";

            var extension = Path.GetExtension(fileUrl).ToLowerInvariant();

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" };
            var videoExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv", ".flv", ".mkv" };

            if (imageExtensions.Contains(extension))
                return "image";

            if (videoExtensions.Contains(extension))
                return "video";

            return "unknown";
        }

        #endregion

        // Helper methods for mock data (replace with real data when available)
        private int GetRandomViews() => new Random().Next(50, 1000);
        private int GetRandomLikes() => new Random().Next(5, 100);
        private int GetRandomComments() => new Random().Next(0, 50);
        private int GetRandomShares() => new Random().Next(0, 25);
    }
}