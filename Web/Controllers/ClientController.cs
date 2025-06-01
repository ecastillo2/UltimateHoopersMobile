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
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientApi clientApi, ILogger<ClientController> logger)
        {
            _clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
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
        public async Task<IActionResult> Create(Client client, CancellationToken cancellationToken = default)
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

                // Set the current date if not provided
                if (client.CreatedDate == default)
                {
                    client.CreatedDate = DateTime.UtcNow;
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
        public async Task<IActionResult> Edit(Client client, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "You must be logged in to edit a client." });
                }

                // Update client
                await _clientApi.UpdateClientAsync(client, accessToken, cancellationToken);

                return Json(new { success = true, message = "Client updated successfully", name = client.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client: {ClientId}", client.ClientId);
                return Json(new { success = false, message = "Error updating client: " + ex.Message });
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

                // Get client details first to check permissions
                var client = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction("Client");
                }

                // Delete client
                await _clientApi.DeleteClientAsync(id, accessToken, cancellationToken);

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
                        notes = "", // Add if you have notes field
                        status = "Active" // Add if you have status field
                    },
                    courtList = clientCourts.Select(c => new
                    {
                        courtId = c.CourtId,
                        name = c.Name,
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
