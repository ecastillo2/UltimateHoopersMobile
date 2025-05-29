using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ApiClients;
using Website.ViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;
using Website.Models;

namespace Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductApi _productApi;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductApi productApi,
            ILogger<ProductController> logger)
        {
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
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view Products.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get current user role and profile ID
                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                // Get Products with cursor pagination
                var result = await _productApi.GetProductsWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                // Create view model
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

                // Transform the data to match what the view expects
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
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view product details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get product details
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
            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a Product.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Product());
        }

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

                //// Handle image upload if provided
                //if (ImageFile != null && ImageFile.Length > 0)
                //{
                //    var imageResult = await ProcessImageUpload(ImageFile);
                //    if (imageResult.Success)
                //    {
                //        product.ImageURL = imageResult.ImageUrl;
                //    }
                //    else
                //    {
                //        TempData["Error"] = imageResult.ErrorMessage;
                //        return View(product);
                //    }
                //}

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
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a Product.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get product details
                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product for edit, ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving the product. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, CancellationToken cancellationToken = default)
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

                if (!ModelState.IsValid)
                {
                    return View(product);
                }

                // Verify user permissions if needed
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Update product
                await _productApi.UpdateProductAsync(product, accessToken, cancellationToken);

                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Details", new { id = product.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Product: {ProductId}", product.ProductId);
                TempData["Error"] = "An error occurred while updating the Product. Please try again later.";
                return View(product);
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
                    TempData["Error"] = "You must be logged in to delete a product.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get product details first to check permissions
                var product = await _productApi.GetProductByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                // Verify user permissions if needed
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Delete product
                await _productApi.DeleteProductAsync(id, accessToken, cancellationToken);

                TempData["Success"] = "Product deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Product: {ProductId}", id);
                TempData["Error"] = "An error occurred while deleting the Product. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        // API endpoint for AJAX requests
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            int page = 1,
            int pageSize = 10,
            string search = "",
            string status = "all",
            string category = "all",
            string type = "all",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Build filter parameters based on the search criteria
                var result = await _productApi.GetProductsWithCursorAsync(
                    cursor: null,
                    limit: pageSize,
                    direction: "next",
                    sortBy: "Title",
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                // Apply client-side filtering if needed
                var filteredProducts = result.Items.AsEnumerable();

                if (!string.IsNullOrEmpty(search))
                {
                    filteredProducts = filteredProducts.Where(p =>
                        (p.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                if (status != "all")
                {
                    filteredProducts = filteredProducts.Where(p =>
                        string.Equals(p.Status, status, StringComparison.OrdinalIgnoreCase));
                }

                if (category != "all")
                {
                    filteredProducts = filteredProducts.Where(p =>
                        string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
                }

                if (type != "all")
                {
                    filteredProducts = filteredProducts.Where(p =>
                        string.Equals(p.Type, type, StringComparison.OrdinalIgnoreCase));
                }

                var paginatedProducts = filteredProducts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = paginatedProducts,
                    totalCount = filteredProducts.Count(),
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for AJAX request");
                return Json(new { success = false, message = "Error loading products" });
            }
        }

        /// <summary>
        /// Handles product image upload via file
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile imageFile, string productId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    return Json(new { success = false, message = "No image file provided" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Invalid file type. Only image files are allowed." });
                }

                // Validate file size (5MB limit)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (imageFile.Length > maxFileSize)
                {
                    return Json(new { success = false, message = "File size exceeds 5MB limit" });
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Define upload path (you may want to configure this in appsettings.json)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file to disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream, cancellationToken);
                }

                // Generate URL for the uploaded image
                var imageUrl = $"/uploads/products/{fileName}";

                // Update product with new image URL (if productId is provided)
                if (!string.IsNullOrEmpty(productId))
                {
                    try
                    {
                        // Get the current product
                        var product = await _productApi.GetProductByIdAsync(productId, accessToken, cancellationToken);
                        if (product != null)
                        {
                            // Update the image URL
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
                    imageUrl = imageUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                return Json(new { success = false, message = "Failed to upload image. Please try again." });
            }
        }

        /// <summary>
        /// Validates an image URL
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

                // Validate URL format
                if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var uri))
                {
                    return Json(new { success = false, message = "Invalid URL format" });
                }

                // Check if URL is accessible
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout

                try
                {
                    using var response = await httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, uri),
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        // Check if it's an image based on content type
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (!string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/"))
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Valid image URL",
                                contentType = contentType,
                                imageUrl = request.ImageUrl
                            });
                        }
                        else
                        {
                            return Json(new { success = false, message = "URL does not point to an image" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Image URL is not accessible" });
                    }
                }
                catch (TaskCanceledException)
                {
                    return Json(new { success = false, message = "Request timed out - URL may be slow or inaccessible" });
                }
                catch (HttpRequestException)
                {
                    return Json(new { success = false, message = "Unable to access the image URL" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image URL: {ImageUrl}", request?.ImageUrl);
                return Json(new { success = false, message = "Error validating image URL" });
            }
        }

       

        /// <summary>
        /// Enhanced Edit method that handles image updates
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

                if (!ModelState.IsValid)
                {
                    return View(product);
                }

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
                    // Try to delete the physical file if it's a local upload
                    if (!string.IsNullOrEmpty(existingProduct.ImageURL) && existingProduct.ImageURL.StartsWith("/uploads/products/"))
                    {
                        try
                        {
                            var fileName = Path.GetFileName(existingProduct.ImageURL);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", fileName);

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not delete physical image file: {ImageUrl}", existingProduct.ImageURL);
                        }
                    }

                    product.ImageURL = null;
                }
                else if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Handle new file upload
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Invalid file type. Only image files are allowed.";
                        return View(product);
                    }

                    // Validate file size (5MB limit)
                    const long maxFileSize = 5 * 1024 * 1024; // 5MB
                    if (ImageFile.Length > maxFileSize)
                    {
                        TempData["Error"] = "File size exceeds 5MB limit.";
                        return View(product);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";

                    // Define upload path
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save new file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream, cancellationToken);
                    }

                    // Delete old file if it exists and is a local upload
                    if (!string.IsNullOrEmpty(existingProduct.ImageURL) && existingProduct.ImageURL.StartsWith("/uploads/products/"))
                    {
                        try
                        {
                            var oldFileName = Path.GetFileName(existingProduct.ImageURL);
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", oldFileName);

                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not delete old image file: {ImageUrl}", existingProduct.ImageURL);
                        }
                    }

                    // Set new image URL
                    product.ImageURL = $"/uploads/products/{fileName}";
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
                return RedirectToAction("Details", new { id = product.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Product: {ProductId}", product.ProductId);
                TempData["Error"] = "An error occurred while updating the Product. Please try again later.";
                return View(product);
            }
        }

    }
}