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
    public class ProductController : Controller
    {
        private readonly IProductApi _productApi;
        private readonly IStorageApi _storageApi;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IStorageApi storageApi,
            IProductApi productApi,
            ILogger<ProductController> logger)
        {
            _storageApi = storageApi ?? throw new ArgumentNullException(nameof(storageApi));
            _productApi = productApi ?? throw new ArgumentNullException(nameof(productApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Product(
             string cursor = null,
             int limit = 10,
             string direction = "next",
             string sortBy = "Title",
             CancellationToken cancellationToken = default)
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

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetProductData(string id, CancellationToken cancellationToken = default)
        {
          
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                var productData = new
                {
                    productId = product.ProductId,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    Points = product.Points,
                    Type = product.Type,
                    Category = product.Category,
                    Status = product.Status,
                    ImageURL = product.ImageURL,
                    ProductNumber = product.ProductNumber,
                    Tag = product.Tag,
                    success = true
                };

                return Json(productData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product data for ID: {productId}", id);
                return Json(new { success = false, message = "Error loading product data" });
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
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a product.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                if (!ModelState.IsValid)
                {
                    return View(product);
                }

                // Handle image upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var imageResult = await ProcessImageUpload(ImageFile);
                    if (imageResult.Success)
                    {
                        product.ImageURL = imageResult.ImageUrl;
                    }
                    else
                    {
                        TempData["Error"] = imageResult.ErrorMessage;
                        return View(product);
                    }
                }

                // Set default values
                product.Status = product.Status ?? "Active";
                product.Points = product.Points ?? 0;

                // Create new Product
                var createdProduct = await _productApi.CreateProductAsync(product, accessToken, cancellationToken);

                TempData["Success"] = "Product created successfully.";
                return RedirectToAction("Details", new { id = createdProduct.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                TempData["Error"] = "An error occurred while creating the product. Please try again later.";
                return View(product);
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

        /// <summary>
        /// Enhanced Edit method that handles image updates comprehensively
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile ImageFile, bool RemoveImage = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a product.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                //if (!ModelState.IsValid)
                //{
                //    return View(product);
                //}

                // Get the existing product to preserve current image if no new image is provided
                var existingProduct = await _productApi.GetProductByIdAsync(product.ProductId, accessToken, cancellationToken);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Product");
                }

                // Handle image removal
                if (RemoveImage)
                {
                    // Delete from blob storage if it's a blob URL
                    //if (!string.IsNullOrEmpty(existingProduct.ImageURL) && IsBlobStorageUrl(existingProduct.ImageURL))
                    //{
                    //    await DeleteBlobImage(existingProduct.ImageURL);
                    //}

                    product.ImageURL = null;
                }
                else if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Handle new file upload
                    var imageResult = await _storageApi.UpdateProductImageFileAsync(product.ProductId,ImageFile); 
                    if (imageResult)
                    {
                        // Delete old image if it exists and is in blob storage
                        //if (!string.IsNullOrEmpty(existingProduct.ImageURL) && IsBlobStorageUrl(existingProduct.ImageURL))
                        //{
                        //    await DeleteBlobImage(existingProduct.ImageURL);
                        //}

                       // product.ImageURL = imageResult.ImageUrl;
                    }
                    else
                    {
                        TempData["Error"] = "Error";
                        return View(product);
                    }
                }
                else if (string.IsNullOrEmpty(product.ImageURL))
                {
                    // No new image provided and no URL specified, keep existing image
                    product.ImageURL = existingProduct.ImageURL;
                }

                // Set default values
                product.Status = product.Status ?? "Active";
                product.Points = product.Points ?? 0;

                // Update product
                await _productApi.UpdateProductAsync(product, accessToken, cancellationToken);

                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Product: {ProductId}", product.ProductId);
                TempData["Error"] = "An error occurred while updating the Product. Please try again later.";
                return View(product);
            }
        }

        /// <summary>
        /// Comprehensive image update handling
        /// </summary>
        private async Task HandleImageUpdate(Product product, Product existingProduct, IFormFile imageFile, bool removeImage)
        {
            try
            {
                // Handle image removal
                if (removeImage)
                {
                    await DeleteExistingImage(existingProduct.ImageURL);
                    product.ImageURL = null;
                    _logger.LogInformation("Image removed for product {ProductId}", product.ProductId);
                    return;
                }

                // Handle new file upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageResult = await ProcessImageUpload(imageFile);
                    if (imageResult.Success)
                    {
                        // Delete old image if it exists and is a local upload
                        if (!string.IsNullOrEmpty(existingProduct.ImageURL))
                        {
                            await DeleteExistingImage(existingProduct.ImageURL);
                        }

                        product.ImageURL = imageResult.ImageUrl;
                        _logger.LogInformation("New image uploaded for product {ProductId}: {ImageUrl}",
                            product.ProductId, product.ImageURL);
                    }
                    else
                    {
                        throw new InvalidOperationException(imageResult.ErrorMessage);
                    }
                }
                else if (!string.IsNullOrEmpty(product.ImageURL))
                {
                    // New URL provided - validate it
                    if (await ValidateImageUrl(product.ImageURL))
                    {
                        // Delete old image if it's different and is a local upload
                        if (!string.IsNullOrEmpty(existingProduct.ImageURL) &&
                            existingProduct.ImageURL != product.ImageURL)
                        {
                            await DeleteExistingImage(existingProduct.ImageURL);
                        }

                        _logger.LogInformation("Image URL updated for product {ProductId}: {ImageUrl}",
                            product.ProductId, product.ImageURL);
                    }
                    else
                    {
                        throw new InvalidOperationException("The provided image URL is not valid or accessible.");
                    }
                }
                else
                {
                    // No new image provided, keep existing
                    product.ImageURL = existingProduct.ImageURL;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling image update for product {ProductId}", product.ProductId);
                throw;
            }
        }

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
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

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
                var imageUrl = $"/uploads/products/{fileName}";

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
        /// Validate image file
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return (false, "No image file provided");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, "Invalid file type. Only image files are allowed (JPG, PNG, GIF, BMP, WEBP).");
            }

            // Validate file size (5MB limit)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (imageFile.Length > maxFileSize)
            {
                return (false, "File size exceeds 5MB limit.");
            }

            // Validate content type
            var allowedContentTypes = new[] {
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "image/bmp", "image/webp"
            };

            if (!allowedContentTypes.Contains(imageFile.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid content type. Only image files are allowed.");
            }

            return (true, null);
        }

        /// <summary>
        /// Validate image URL
        /// </summary>
        private async Task<bool> ValidateImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                return false;

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                using var response = await httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, uri));

                if (!response.IsSuccessStatusCode)
                    return false;

                var contentType = response.Content.Headers.ContentType?.MediaType;
                return !string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating image URL: {ImageUrl}", imageUrl);
                return false;
            }
        }

        /// <summary>
        /// Delete existing image file
        /// </summary>
        private async Task DeleteExistingImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/uploads/products/"))
                return;

            try
            {
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    await Task.Run(() => System.IO.File.Delete(filePath));
                    _logger.LogInformation("Deleted image file: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete image file: {ImageUrl}", imageUrl);
                // Don't throw - this is not critical
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
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Product");
                }

                // Delete associated image file
                if (!string.IsNullOrEmpty(product.ImageURL))
                {
                    await DeleteExistingImage(product.ImageURL);
                }

                // Delete product
                await _productApi.DeleteProductAsync(id, accessToken, cancellationToken);

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

        /// <summary>
        /// API endpoint for AJAX image validation
        /// </summary>
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

                var isValid = await ValidateImageUrl(request.ImageUrl);

                if (isValid)
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
                    return Json(new { success = false, message = "Invalid or inaccessible image URL" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image URL: {ImageUrl}", request?.ImageUrl);
                return Json(new { success = false, message = "Error validating image URL" });
            }
        }

        /// <summary>
        /// Handles product image upload via AJAX
        /// </summary>
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

                var uploadResult = await ProcessImageUpload(imageFile);
                if (!uploadResult.Success)
                {
                    return Json(new { success = false, message = uploadResult.ErrorMessage });
                }

                // Update product with new image URL if productId is provided
                if (!string.IsNullOrEmpty(productId))
                {
                    try
                    {
                        var product = await _productApi.GetProductByIdAsync(productId, accessToken, cancellationToken);
                        if (product != null)
                        {
                            // Delete old image if it exists
                            if (!string.IsNullOrEmpty(product.ImageURL))
                            {
                                await DeleteExistingImage(product.ImageURL);
                            }

                            product.ImageURL = uploadResult.ImageUrl;
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
                    imageUrl = uploadResult.ImageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                return Json(new { success = false, message = "Failed to upload image. Please try again." });
            }
        }
    }
}