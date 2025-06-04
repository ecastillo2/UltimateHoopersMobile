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
    public class GameController : Controller
    {
        private readonly IGameApi _gameApi;
     
        private readonly ILogger<GameController> _logger;

        public GameController(IGameApi gameApi, ILogger<GameController> logger)
        {
           
            _gameApi = gameApi ?? throw new ArgumentNullException(nameof(gameApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Game(string cursor = null, int limit = 10, string direction = "next", string sortBy = "Title", CancellationToken cancellationToken = default)
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

                var result = await _gameApi.GetGamesWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                var viewModel = new GamesViewModel
                {
                    GameList = result.Items,
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
        public async Task<IActionResult> GetGameData(string id, CancellationToken cancellationToken = default)
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

                var product = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                var productData = new
                {
                    success = true,
                    product = new
                    {
                        gameId = product.GameId,
        
                        gameNumber = product.GameNumber,
                   
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

                var product = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
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
        public async Task<IActionResult> Create(Game product, IFormFile ImageFile, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }


              
                // Set default values
                product.GameId = Guid.NewGuid().ToString();
                product.GameNumber = UniqueIdNumber.GenerateSixDigit();

                // Create product
                var createdProduct = await _gameApi.CreateGameAsync(product, accessToken, cancellationToken);

                if (createdProduct != null)
                {
                    _logger.LogInformation("Product created successfully: {ProductId}", product.GameId);

                    return Json(new
                    {
                        success = true,
                        message = "Product created successfully!",
                        product = new
                        {
                            gameId = product.GameId,
                            gameNumber = product.GameNumber,
                           
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

                var product = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
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
        public async Task<IActionResult> Edit(Game product, IFormFile ImageFile, bool RemoveImage = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(product.GameId))
                {
                    return Json(new { success = false, message = "Product ID is required" });
                }

                // Get existing product
                var existingProduct = await _gameApi.GetGameByIdAsync(product.GameId, accessToken, cancellationToken);
                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }


                // Set default values
               
                product.GameNumber = existingProduct.GameNumber; // Preserve original product number

                // Update product
                await _gameApi.UpdateGameAsync(product, accessToken, cancellationToken);

                _logger.LogInformation("Product updated successfully: {ProductId}", product.GameId);

                return Json(new
                {
                    success = true,
                    message = "Product updated successfully!",
                    product = new
                    {
                        productId = product.GameId,
                      
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Product: {ProductId}", product?.GameId);
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
                var product = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);


                // Delete product
                var result = await _gameApi.DeleteGameAsync(id, accessToken, cancellationToken);



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



    }

     

   
     
  
  
}