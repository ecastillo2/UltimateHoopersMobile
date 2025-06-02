using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Mvc;
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
                    TempData["Error"] = "You must be logged in to view runs.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get current user role and profile ID
                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                // Get runs with cursor pagination
                var result = await _postApi.GetPostsAsync(postType, accessToken, cancellationToken = default);

                // Create view model
                var viewModel = new PostsViewModel
                {
                    PostList = result,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving runs");
                TempData["Error"] = "An error occurred while retrieving runs. Please try again later.";
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
                        content = "",
                        description = "",
                        postType = post.PostType,
                        status = post.Status,            
                        imageURL = post.PostFileURL,
                        thumbnailURL = post.ThumbnailUrl,
                        postedDate = post.PostedDate?.ToString("yyyy-MM-ddTHH:mm") ?? "",
                        startDate =  "",
                        endDate =  "",
                        profileId = post.ProfileId,
                        author = post.ProfileId ?? "System",
                        isActive = post.Status?.ToLower() == "active",
                        createdDate = post.PostedDate?.ToString("yyyy-MM-ddTHH:mm") ?? "",
                        views = GetRandomViews(),
                        likes = GetRandomLikes(),
                        comments = GetRandomComments(),
                        shares = GetRandomShares()
                    },
                    // Additional metadata
                    metadata = new
                    {
                        hasImage = !string.IsNullOrEmpty(post.PostFileURL),
                        hasContent = !string.IsNullOrEmpty("post.Content"),
                        hasDescription = !string.IsNullOrEmpty("post.Description"),
                       
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
                    TempData["Error"] = "You must be logged in to view run details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get run details
                var run = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                return View(run);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run details for ID: {RunId}", id);
                TempData["Error"] = "An error occurred while retrieving run details. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a run.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Run());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post run, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Set creator profile ID
                run.ProfileId = HttpContext.Session.GetString("ProfileId");

                // Create new run
                var createdRun = await _postApi.CreatePostAsync(run, accessToken, cancellationToken);

                TempData["Success"] = "Run created successfully.";
                return RedirectToAction("Details", new { id = createdRun.PostId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating run");
                TempData["Error"] = "An error occurred while creating the run. Please try again later.";
                return View(run);
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
                    TempData["Error"] = "You must be logged in to edit a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get run details
                var run = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to edit this run.";
                    return RedirectToAction("Details", new { id = id });
                }

                return View(run);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving run for edit, ID: {RunId}", id);
                TempData["Error"] = "An error occurred while retrieving the run. Please try again later.";
                return RedirectToAction("Index");
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
                    TempData["Error"] = "You must be logged in to delete a run.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get run details first to check permissions
                var run = await _postApi.GetPostByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                if (run.ProfileId != profileId && userRole != "Admin")
                {
                    TempData["Error"] = "You do not have permission to delete this run.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Delete run
                await _postApi.DeletePostAsync(id, accessToken, cancellationToken);

                TempData["Success"] = "Run deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting run: {RunId}", id);
                TempData["Error"] = "An error occurred while deleting the run. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        // Helper methods for mock data (replace with real data when available)
        private int GetRandomViews() => new Random().Next(50, 1000);
        private int GetRandomLikes() => new Random().Next(5, 100);
        private int GetRandomComments() => new Random().Next(0, 50);
        private int GetRandomShares() => new Random().Next(0, 25);
    }
}