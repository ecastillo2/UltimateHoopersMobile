using Common;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ApiClients;
using Website.ViewModels;

namespace Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductApi _productApi;
        private readonly IStorageApi _storageApi;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IStorageApi storageApi, IProductApi productApi, ILogger<ProductController> logger)
        {
            _storageApi = storageApi ?? throw new ArgumentNullException(nameof(storageApi));
            _productApi = productApi ?? throw new ArgumentNullException(nameof(productApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Product
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Product(string cursor = null, int limit = 10, string direction = "next", string sortBy = "Title", CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view Products.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                var result = await _productApi.GetProductsWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                var viewModel = new ProductsViewModel
                {
                    ProductList = result.Items,
                    NextCursor = result.NextCursor,
                    CurrentSortBy = sortBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Products");
                TempData["Error"] = "An error occurred while retrieving Products. Please try again later.";
                return RedirectToAction("Dashboard", "Dashboard");
            }
        }

        /// <summary>
        /// Get Product Data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductData(string id, CancellationToken cancellationToken = default)
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
                    return Json(new { success = false, message = "Product ID is required" });
                }

                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                var productData = new
                {
                    success = true,
                    product = new
                    {
                        productId = product.ProductId,
                        title = product.Title,
                        description = product.Description,
                        price = product.Price,
                        points = product.Points,
                        type = product.Type,
                        category = product.Category,
                        status = product.Status,
                        imageURL = product.ImageURL,
                        productNumber = product.ProductNumber,
                        tag = product.Tag
                    }
                };

                return Json(productData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product data for ID: {productId}", id);
                return Json(new { success = false, message = "Error loading product data: " + ex.Message });
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
                    TempData["Error"] = "You must be logged in to view product details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Product");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Product details for ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving Product details. Please try again later.";
                return RedirectToAction("Product");
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
        public async Task<IActionResult> Create(Product product, IFormFile ImageFile, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                // Enhanced validation
                var validationResult = ValidateProduct(product);
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
                product.ProductId = Guid.NewGuid().ToString();
                product.ProductNumber = UniqueIdNumber.GenerateSixDigit();
                product.Status = product.Status ?? "Active";
                product.Points = product.Points ?? 0;

                // Handle image file upload
                string imageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        product.ImageUrlName = product.ProductId + Path.GetExtension(ImageFile.FileName).ToLower();
                        var uploadResult = await _storageApi.UpdateProductImageFileAsync(product.ProductId, ImageFile);

                        if (uploadResult)
                        {
                            // Set the image URL - you may need to construct this based on your storage configuration
                            imageUrl = $"/api/storage/product/{product.ImageUrlName}"; // Adjust based on your storage setup
                            product.ImageURL = imageUrl;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to upload image. Please try again." });
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading image for product: {ProductId}", product.ProductId);
                        return Json(new { success = false, message = "Error uploading image: " + uploadEx.Message });
                    }
                }
                else if (!string.IsNullOrEmpty(product.ImageURL))
                {
                    // Validate image URL if provided
                    var urlValidation = await ValidateImageUrl(product.ImageURL);
                    if (!urlValidation.IsValid)
                    {
                        return Json(new { success = false, message = urlValidation.ErrorMessage });
                    }
                }

                // Create product
                var createdProduct = await _productApi.CreateProductAsync(product, accessToken, cancellationToken);

                if (createdProduct != null)
                {
                    _logger.LogInformation("Product created successfully: {ProductId}", product.ProductId);

                    return Json(new
                    {
                        success = true,
                        message = "Product created successfully!",
                        product = new
                        {
                            productId = product.ProductId,
                            title = product.Title,
                            productNumber = product.ProductNumber,
                            price = product.Price,
                            points = product.Points,
                            status = product.Status,
                            category = product.Category,
                            type = product.Type,
                            imageURL = product.ImageURL,
                            description = product.Description,
                            tag = product.Tag
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create product. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return Json(new { success = false, message = "An error occurred while creating the product: " + ex.Message });
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
                    TempData["Error"] = "You must be logged in to edit a Product.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Product");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product for edit, ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving the product. Please try again later.";
                return RedirectToAction("Product");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile ImageFile, bool RemoveImage = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(product.ProductId))
                {
                    return Json(new { success = false, message = "Product ID is required" });
                }

                // Enhanced validation
                var validationResult = ValidateProduct(product);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage, field = validationResult.Field });
                }

                // Get existing product
                var existingProduct = await _productApi.GetProductByIdAsync(product.ProductId, accessToken, cancellationToken);
                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Handle image operations
                string imageUrl = existingProduct.ImageURL;

                if (RemoveImage)
                {
                    // Remove image
                    imageUrl = null;
                    product.ImageURL = null;
                }
                else if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Validate and upload new image
                    var fileValidation = ValidateImageFile(ImageFile);
                    if (!fileValidation.IsValid)
                    {
                        return Json(new { success = false, message = fileValidation.ErrorMessage });
                    }

                    try
                    {
                        var uploadResult = await _storageApi.UpdateProductImageFileAsync(product.ProductId, ImageFile);
                        if (uploadResult)
                        {
                            imageUrl = $"/api/storage/product/{product.ProductId}{Path.GetExtension(ImageFile.FileName).ToLower()}";
                            product.ImageURL = imageUrl;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to upload image. Please try again." });
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading image for product: {ProductId}", product.ProductId);
                        return Json(new { success = false, message = "Error uploading image: " + uploadEx.Message });
                    }
                }
                else if (!string.IsNullOrEmpty(product.ImageURL) && product.ImageURL != existingProduct.ImageURL)
                {
                    // Validate new image URL
                    var urlValidation = await ValidateImageUrl(product.ImageURL);
                    if (!urlValidation.IsValid)
                    {
                        return Json(new { success = false, message = urlValidation.ErrorMessage });
                    }
                    imageUrl = product.ImageURL;
                }
                else
                {
                    // Keep existing image
                    product.ImageURL = existingProduct.ImageURL;
                }

                // Set default values
                product.Status = product.Status ?? "Active";
                product.Points = product.Points ?? 0;
                product.ProductNumber = existingProduct.ProductNumber; // Preserve original product number

                // Update product
                await _productApi.UpdateProductAsync(product, accessToken, cancellationToken);

                _logger.LogInformation("Product updated successfully: {ProductId}", product.ProductId);

                return Json(new
                {
                    success = true,
                    message = "Product updated successfully!",
                    product = new
                    {
                        productId = product.ProductId,
                        title = product.Title,
                        productNumber = product.ProductNumber,
                        price = product.Price,
                        points = product.Points,
                        status = product.Status,
                        category = product.Category,
                        type = product.Type,
                        imageURL = product.ImageURL,
                        description = product.Description,
                        tag = product.Tag
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Product: {ProductId}", product?.ProductId);
                return Json(new { success = false, message = "An error occurred while updating the product: " + ex.Message });
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

                // Get product details first to clean up image
                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                

                    // Delete product
                    var result = await _productApi.DeleteProductAsync(id, accessToken, cancellationToken);

                if (result.Success)
                {
                    var uploadResult = await _storageApi.RemoveProductImageFileAsync($"{product.ProductId}.webp");
                    
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
        public async Task<IActionResult> ValidateImageUrl([FromBody] ValidateImageUrlRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.ImageUrl))
                {
                    return Json(new { success = false, message = "No URL provided" });
                }

                var validation = await ValidateImageUrl(request.ImageUrl);

                if (validation.IsValid)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Valid image URL",
                        imageUrl = request.ImageUrl
                    });
                }
                else
                {
                    return Json(new { success = false, message = validation.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image URL: {ImageUrl}", request?.ImageUrl);
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
                        var product = await _productApi.GetProductByIdAsync(productId, accessToken, cancellationToken);
                        if (product != null)
                        {
                            product.ImageURL = imageUrl;
                            await _productApi.UpdateProductAsync(product, accessToken, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating product image URL for product: {ProductId}", productId);
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
                _logger.LogError(ex, "Error uploading product image");
                return Json(new { success = false, message = "Failed to upload image. Please try again." });
            }
        }

        // ========== VALIDATION METHODS ==========

        private ValidationResult ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Title))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Product title is required", Field = "Title" };
            }

            if (product.Title.Length > 100)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Product title cannot exceed 100 characters", Field = "Title" };
            }

            if (product.Price.HasValue && product.Price < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Price cannot be negative", Field = "Price" };
            }

            if (product.Points.HasValue && product.Points < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Points cannot be negative", Field = "Points" };
            }

            if (string.IsNullOrWhiteSpace(product.Type))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Product type is required", Field = "Type" };
            }

            if (string.IsNullOrWhiteSpace(product.Category))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Product category is required", Field = "Category" };
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

        private async Task<ValidationResult> ValidateImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Image URL is required" };
            }

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uri))
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
                _logger.LogWarning(ex, "Error validating image URL: {ImageUrl}", imageUrl);
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

    public class ValidateImageUrlRequest
    {
        public string ImageUrl { get; set; }
    }
}