using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ApiClients;
using Website.ViewModels;

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
                    Price = "$"+product.Price,
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
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Product details for ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving Product details. Please try again later.";
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
                TempData["Error"] = "You must be logged in to create a Product.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, CancellationToken cancellationToken = default)
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

                //if (!string.IsNullOrEmpty(search))
                //{
                //    filteredProducts = filteredProducts.Where(p =>
                //        (p.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                //        (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
                //}

                //if (status != "all")
                //{
                //    filteredProducts = filteredProducts.Where(p =>
                //        string.Equals(p.Status, status, StringComparison.OrdinalIgnoreCase));
                //}

                //if (category != "all")
                //{
                //    filteredProducts = filteredProducts.Where(p =>
                //        string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
                //}

                //if (type != "all")
                //{
                //    filteredProducts = filteredProducts.Where(p =>
                //        string.Equals(p.Type, type, StringComparison.OrdinalIgnoreCase));
                //}

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
    }
}