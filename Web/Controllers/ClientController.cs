using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.ApiClients;
using Website.Attributes;
using Website.ViewModels;

namespace Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientApi _clientApi;
        private readonly IStorageApi _storageApi;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientApi clientApi, IStorageApi storageApi, ILogger<ClientController> logger)
        {
            _clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
            _storageApi = storageApi ?? throw new ArgumentNullException(nameof(storageApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authentication("Admin")] // Only admins can view clients
        public async Task<IActionResult> Client(string cursor = null, int limit = 10, string direction = "next", string sortBy = "CreatedDate", CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view clients.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get current user role and profile ID
                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                // Get clients with cursor pagination
                var result = await _clientApi.GetClientsWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                // Create view model
                var viewModel = new ClientsViewModel
                {
                    Clients = result.Items,
                    NextCursor = result.NextCursor,
                    CurrentSortBy = sortBy,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                TempData["Error"] = "An error occurred while retrieving clients. Please try again later.";
                return RedirectToAction("Dashboard", "Dashboard");
            }
        }

        [HttpGet]
        [Authentication("Admin")]
        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view client details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get client details
                var client = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction("Client");
                }

                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client details for ID: {ClientId}", id);
                TempData["Error"] = "An error occurred while retrieving client details. Please try again later.";
                return RedirectToAction("Client");
            }
        }

        [HttpGet]
        [Authentication("Admin")]
        public IActionResult Create()
        {
            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a client.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Client());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> Create(Client client, IFormFile ImageFile, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to create a client.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Enhanced validation
                var validationResult = ValidateClient(client);
                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.ErrorMessage;
                    return View(client);
                }

                // Validate image file if provided
                if (ImageFile != null)
                {
                    var fileValidation = ValidateImageFile(ImageFile);
                    if (!fileValidation.IsValid)
                    {
                        TempData["Error"] = fileValidation.ErrorMessage;
                        return View(client);
                    }
                }

                // Set the current date if not provided
                if (client.CreatedDate == default)
                {
                    client.CreatedDate = DateTime.UtcNow;
                }

                // Handle image file upload
                string imageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        client.ImageUrl = client.ClientId + Path.GetExtension(ImageFile.FileName).ToLower();
                        var uploadResult = await _storageApi.UpdateClientImageFileAsync(client.ClientId, ImageFile);

                        if (uploadResult)
                        {
                            imageUrl = $"/api/storage/client/{client.ImageUrl}";
                            client.ImageUrl = imageUrl;
                        }
                        else
                        {
                            TempData["Error"] = "Failed to upload image. Please try again.";
                            return View(client);
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading image for client: {ClientId}", client.ClientId);
                        TempData["Error"] = "Error uploading image: " + uploadEx.Message;
                        return View(client);
                    }
                }
                else if (!string.IsNullOrEmpty(client.ImageUrl))
                {
                    // Validate image URL if provided
                    var urlValidation = await ValidateImageUrl(client.ImageUrl);
                    if (!urlValidation.IsValid)
                    {
                        TempData["Error"] = urlValidation.ErrorMessage;
                        return View(client);
                    }
                }

                // Create new client
                var createdClient = await _clientApi.CreateClientAsync(client, accessToken, cancellationToken);

                TempData["Success"] = "Client created successfully.";
                return RedirectToAction("Client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                TempData["Error"] = "An error occurred while creating the client. Please try again later.";
                return View(client);
            }
        }

        [HttpGet]
        [Authentication("Admin")]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a client.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get client details
                var client = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction("Client");
                }

                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client for edit, ID: {ClientId}", id);
                TempData["Error"] = "An error occurred while retrieving the client. Please try again later.";
                return RedirectToAction("Client");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> Edit(Client client, IFormFile ImageFile, bool RemoveImage = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Log the incoming request for debugging
                _logger.LogInformation("Edit request received for client ID: {ClientId}", client?.ClientId);

                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("No access token found in session for client edit");

                    // Check if this is an AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "You must be logged in to edit a client." });
                    }
                    else
                    {
                        TempData["Error"] = "You must be logged in to edit a client.";
                        return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                    }
                }

                // Validate the client object
                if (client == null)
                {
                    _logger.LogWarning("Client object is null in edit request");
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Invalid client data provided." });
                    }
                    else
                    {
                        TempData["Error"] = "Invalid client data provided.";
                        return RedirectToAction("Client");
                    }
                }

                if (string.IsNullOrEmpty(client.ClientId))
                {
                    _logger.LogWarning("Client ID is missing in edit request");
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Client ID is required." });
                    }
                    else
                    {
                        TempData["Error"] = "Client ID is required.";
                        return RedirectToAction("Client");
                    }
                }

                // Enhanced validation
                var validationResult = ValidateClient(client);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Client validation failed: {Error}", validationResult.ErrorMessage);
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = validationResult.ErrorMessage, field = validationResult.Field });
                    }
                    else
                    {
                        TempData["Error"] = validationResult.ErrorMessage;
                        return View(client);
                    }
                }

                // Get existing client
                var existingClient = await _clientApi.GetClientByIdAsync(client.ClientId, accessToken, cancellationToken);
                if (existingClient == null)
                {
                    _logger.LogError("Existing client not found for ID: {ClientId}", client.ClientId);
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Client not found" });
                    }
                    else
                    {
                        TempData["Error"] = "Client not found.";
                        return RedirectToAction("Client");
                    }
                }

                // Handle image operations
                string imageUrl = existingClient.ImageUrl;

                if (RemoveImage)
                {
                    _logger.LogInformation("Removing image for client: {ClientId}", client.ClientId);
                    imageUrl = null;
                    client.ImageUrl = null;
                }
                else if (ImageFile != null && ImageFile.Length > 0)
                {
                    _logger.LogInformation("Processing new image upload for client: {ClientId}", client.ClientId);

                    // Validate and upload new image
                    var fileValidation = ValidateImageFile(ImageFile);
                    if (!fileValidation.IsValid)
                    {
                        _logger.LogWarning("Image file validation failed: {Error}", fileValidation.ErrorMessage);
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = fileValidation.ErrorMessage });
                        }
                        else
                        {
                            TempData["Error"] = fileValidation.ErrorMessage;
                            return View(client);
                        }
                    }

                    try
                    {
                        var uploadResult = await _storageApi.UpdateClientImageFileAsync(client.ClientId, ImageFile);
                        if (uploadResult)
                        {
                            imageUrl = $"/api/storage/client/{client.ClientId}{Path.GetExtension(ImageFile.FileName).ToLower()}";
                            client.ImageUrl = imageUrl;
                            _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);
                        }
                        else
                        {
                            _logger.LogError("Failed to upload image for client: {ClientId}", client.ClientId);
                            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                return Json(new { success = false, message = "Failed to upload image. Please try again." });
                            }
                            else
                            {
                                TempData["Error"] = "Failed to upload image. Please try again.";
                                return View(client);
                            }
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Error uploading image for client: {ClientId}", client.ClientId);
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Error uploading image: " + uploadEx.Message });
                        }
                        else
                        {
                            TempData["Error"] = "Error uploading image: " + uploadEx.Message;
                            return View(client);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(client.ImageUrl) && client.ImageUrl != existingClient.ImageUrl)
                {
                    _logger.LogInformation("Validating new image URL for client: {ClientId}", client.ClientId);

                    // Validate new image URL
                    var urlValidation = await ValidateImageUrl(client.ImageUrl);
                    if (!urlValidation.IsValid)
                    {
                        _logger.LogWarning("Image URL validation failed: {Error}", urlValidation.ErrorMessage);
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = urlValidation.ErrorMessage });
                        }
                        else
                        {
                            TempData["Error"] = urlValidation.ErrorMessage;
                            return View(client);
                        }
                    }
                    imageUrl = client.ImageUrl;
                }
                else
                {
                    // Keep existing image
                    client.ImageUrl = existingClient.ImageUrl;
                }

                // Preserve original client number and created date
                client.ClientNumber = existingClient.ClientNumber;
                client.CreatedDate = existingClient.CreatedDate;

                // Log the client data being processed
                _logger.LogInformation("Processing client update: {ClientId} - {ClientName}", client.ClientId, client.Name);

                // Update client
                await _clientApi.UpdateClientAsync(client, accessToken, cancellationToken);

                _logger.LogInformation("Client updated successfully: {ClientId}", client.ClientId);

                // Return appropriate response based on request type
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Client updated successfully",
                        client = new
                        {
                            clientId = client.ClientId,
                            name = client.Name,
                            clientNumber = client.ClientNumber,
                            address = client.Address,
                            city = client.City,
                            state = client.State,
                            zip = client.Zip,
                            phoneNumber = client.PhoneNumber,
                            imageUrl = client.ImageUrl,
                            notes = "",
                            createdDate = client.CreatedDate
                        }
                    });
                }
                else
                {
                    TempData["Success"] = "Client updated successfully.";
                    return RedirectToAction("Client");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client: {ClientId} - {ErrorMessage}", client?.ClientId, ex.Message);

                // Return appropriate error response based on request type
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error updating client: " + ex.Message,
                        details = ex.InnerException?.Message // Optional: include inner exception for debugging
                    });
                }
                else
                {
                    TempData["Error"] = "Error updating client: " + ex.Message;
                    return View(client);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to delete a client.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get client details first to check permissions and clean up image
                var client = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction("Client");
                }

                // Delete client
                await _clientApi.DeleteClientAsync(id, accessToken, cancellationToken);

                // Clean up client image if exists
                if (!string.IsNullOrEmpty(client.ImageUrl))
                {
                    try
                    {
                        await _storageApi.RemoveClientImageFileAsync($"{client.ClientId}.webp");
                    }
                    catch (Exception imgEx)
                    {
                        _logger.LogWarning(imgEx, "Failed to delete client image for: {ClientId}", client.ClientId);
                    }
                }

                TempData["Success"] = "Client deleted successfully.";
                return RedirectToAction("Client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client: {ClientId}", id);
                TempData["Error"] = "An error occurred while deleting the client. Please try again later.";
                return RedirectToAction("Client");
            }
        }

        // Image upload and validation endpoints
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> UploadClientImage(IFormFile imageFile, string clientId, CancellationToken cancellationToken = default)
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

                var uploadResult = await _storageApi.UpdateClientImageFileAsync(clientId, imageFile);
                if (!uploadResult)
                {
                    return Json(new { success = false, message = "Failed to upload image" });
                }

                var imageUrl = $"/api/storage/client/{clientId}{Path.GetExtension(imageFile.FileName).ToLower()}";

                // Update client with new image URL if clientId is provided
                if (!string.IsNullOrEmpty(clientId))
                {
                    try
                    {
                        var client = await _clientApi.GetClientByIdAsync(clientId, accessToken, cancellationToken);
                        if (client != null)
                        {
                            client.ImageUrl = imageUrl;
                            await _clientApi.UpdateClientAsync(client, accessToken, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating client image URL for client: {ClientId}", clientId);
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
                _logger.LogError(ex, "Error uploading client image");
                return Json(new { success = false, message = "Failed to upload image. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> ValidateClientImageUrl([FromBody] ValidateImageUrlRequest request, CancellationToken cancellationToken = default)
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

        // Client data retrieval for AJAX requests - similar to UserController
        [HttpGet]
        [Authentication("Admin")]
        public async Task<IActionResult> GetClientData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Get client details
                var client = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }

                // Get client courts (if available in your API)
                var clientCourts = new List<Court>();
                try
                {
                    // If you have a method to get courts, use it here
                    clientCourts = await _clientApi.GetClientCourtsAsync(id, accessToken, cancellationToken);
                }
                catch (Exception courtEx)
                {
                    _logger.LogError(courtEx, "Error retrieving courts for client: {ClientId}", id);
                    // Continue with empty courts list
                }

                // Format the response similar to UserController
                var clientData = new
                {
                    success = true,
                    client = new
                    {
                        clientId = client.ClientId,
                        clientNumber = client.ClientNumber,
                        name = client.Name,
                        address = client.Address,
                        city = client.City,
                        state = client.State,
                        zip = client.Zip,
                        phoneNumber = client.PhoneNumber,
                        createdDate = client.CreatedDate,
                        imageUrl = client.ImageUrl,
                        notes =  "", // Add if you have notes field
                        status = "Active" // Add if you have status field
                    },
                    courtList = clientCourts.Select(c => new
                    {
                        courtId = c.CourtId,
                        name = c.Name,
                        courtNumber = c.CourtNumber,
                        address = c.Address ?? "",
                        isIndoor = true,
                        isActive = true
                    }).ToList()
                };

                return Json(clientData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client data for ID: {ClientId}", id);
                return Json(new { success = false, message = "Error retrieving client data" });
            }
        }

        // Add method for getting client business information (similar to user profile)
        [HttpGet]
        [Authentication("Admin")]
        public async Task<IActionResult> GetClientBusinessData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // In a real implementation, you would fetch this from your database
                var businessData = new
                {
                    businessName = "Sample Business",
                    businessType = "Sports Facility",
                    establishedDate = DateTime.Now.AddYears(-5),
                    employeeCount = "10-50",
                    annualRevenue = "$100k-$500k",
                    primaryServices = new[] { "Court Rental", "Training", "Events" },
                    operatingHours = "6 AM - 10 PM",
                    website = "https://example.com",
                    socialMedia = new { facebook = "", instagram = "", twitter = "" },
                    stats = new
                    {
                        totalBookings = 1250,
                        monthlyUsers = 180,
                        averageRating = 4.5,
                        repeatCustomers = 85
                    }
                };

                return Json(new { success = true, business = businessData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client business data for ID: {ClientId}", id);
                return Json(new { success = false, message = "Error retrieving business data: " + ex.Message });
            }
        }

        // Get client activity/usage data
        [HttpGet]
        [Authentication("Admin")]
        public IActionResult GetClientActivity(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // In a real implementation, you would fetch this from your database
                var activities = new List<object>
            {
                new {
                    type = "court_booking",
                    title = "Court Booked",
                    description = "Basketball Court A reserved for 2 hours",
                    timestamp = DateTime.UtcNow.AddDays(-1)
                },
                new {
                    type = "payment",
                    title = "Payment Received",
                    description = "Monthly subscription payment processed",
                    timestamp = DateTime.UtcNow.AddDays(-5)
                },
                new {
                    type = "event",
                    title = "Event Hosted",
                    description = "Youth Basketball Tournament organized",
                    timestamp = DateTime.UtcNow.AddDays(-10)
                },
                new {
                    type = "maintenance",
                    title = "Court Maintenance",
                    description = "Routine maintenance completed on Court B",
                    timestamp = DateTime.UtcNow.AddDays(-15)
                },
                new {
                    type = "contract_renewal",
                    title = "Contract Renewed",
                    description = "Annual service contract renewed",
                    timestamp = DateTime.UtcNow.AddDays(-30)
                }
            };

                return Json(new { activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client activity for ID: {ClientId}", id);
                return Json(new { success = false, message = "Error retrieving client activity: " + ex.Message });
            }
        }

        // Court management endpoints
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> AddClientCourt([FromBody] AddCourtRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // For now, return success - implement actual API call when available
                // await _clientApi.AddCourtAsync(request.ClientId, request.Name, accessToken, cancellationToken);

                return Json(new { success = true, message = "Court added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding court for client: {ClientId}", request?.ClientId);
                return Json(new { success = false, message = "Error adding court: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> UpdateClientCourt([FromBody] UpdateCourtRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // For now, return success - implement actual API call when available
                // await _clientApi.UpdateCourtAsync(request.CourtId, request.Name, accessToken, cancellationToken);

                return Json(new { success = true, message = "Court updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating court: {CourtId}", request?.CourtId);
                return Json(new { success = false, message = "Error updating court: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication("Admin")]
        public async Task<IActionResult> RemoveClientCourt([FromBody] RemoveCourtRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // For now, return success - implement actual API call when available
                // await _clientApi.RemoveCourtAsync(request.CourtId, accessToken, cancellationToken);

                return Json(new { success = true, message = "Court removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing court: {CourtId}", request?.CourtId);
                return Json(new { success = false, message = "Error removing court: " + ex.Message });
            }
        }

        // ========== VALIDATION METHODS ==========

        private ValidationResult ValidateClient(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Name))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Client name is required", Field = "Name" };
            }

            if (client.Name.Length > 100)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Client name cannot exceed 100 characters", Field = "Name" };
            }

            if (string.IsNullOrWhiteSpace(client.Address))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Address is required", Field = "Address" };
            }

            if (string.IsNullOrWhiteSpace(client.City))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "City is required", Field = "City" };
            }

            if (string.IsNullOrWhiteSpace(client.Zip))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Zip code is required", Field = "Zip" };
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

    // Request models for court management
    public class AddCourtRequest
    {
        public string ClientId { get; set; }
        public string Name { get; set; }
    }

    public class UpdateCourtRequest
    {
        public string CourtId { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
    }

    public class RemoveCourtRequest
    {
        public string ClientId { get; set; }
        public string CourtId { get; set; }
    }

    
}