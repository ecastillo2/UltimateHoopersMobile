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
using Website.ViewModels;

namespace Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientApi _clientApi;
        private readonly ILogger<ClientController> _logger;

        public ClientController(
            IClientApi clientApi,
            ILogger<ClientController> logger)
        {
            _clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Client(
            string cursor = null,
            int limit = 10,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
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
        public async Task<IActionResult> Edit(Client client, CancellationToken cancellationToken = default)
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



                // Update client
                await _clientApi.UpdateClientAsync(client, accessToken, cancellationToken);

                TempData["Success"] = "Client updated successfully.";
                return RedirectToAction("Client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client: {ClientId}", client.ClientId);
                TempData["Error"] = "An error occurred while updating the client. Please try again later.";
                return View(client);
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

        // Client data retrieval for AJAX requests
        [HttpGet]
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
                    // clientCourts = await _clientApi.GetClientCourtsAsync(id, accessToken, cancellationToken);

                    // For mock purposes, we'll create some sample courts
                    if (client.ClientId != null && client.ClientId.Length > 5)
                    {
                        clientCourts.Add(new Court
                        {
                            CourtId = Guid.NewGuid().ToString(),
                            ClientId = client.ClientId,
                            Name = "Main Street Courts",
                            Location = "Downtown",
                            Type = "Indoor",
                            IsPreferred = true
                        });

                        clientCourts.Add(new Court
                        {
                            CourtId = Guid.NewGuid().ToString(),
                            ClientId = client.ClientId,
                            Name = "Westside Park Courts",
                            Location = "West Side",
                            Type = "Outdoor",
                            IsPreferred = false
                        });
                    }
                }
                catch (Exception courtEx)
                {
                    _logger.LogError(courtEx, "Error retrieving courts for client: {ClientId}", id);
                    // Continue with empty courts list
                }





                // Format the response
                var clientData = new
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
                    courtList = clientCourts.Select(c => new
                    {
                        courtId = c.CourtId,
                        name = c.Name,
                        location = c.Location,
                        type = c.Type,
                        isPreferred = c.IsPreferred
                    }).ToList(),
                    success = true
                };

                return Json(clientData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client data for ID: {ClientId}", id);
                return Json(new { success = false, message = "Error retrieving client data" });
            }
        }

        // Court management API endpoints
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClientCourt([FromBody] CourtDto courtData, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Create a Court object from the DTO
                var court = new Court
                {
                    CourtId = Guid.NewGuid().ToString(),
                    ClientId = courtData.ClientId,
                    Name = courtData.Name,
                    Location = courtData.Location,
                    Type = courtData.Type,
                    IsPreferred = courtData.IsPreferred
                };

                // Add court to client (replace with your actual API call)
                // var addedCourt = await _clientApi.AddClientCourtAsync(court, accessToken, cancellationToken);

                // For now, mock the response
                var addedCourt = court;

                return Json(new
                {
                    success = true,
                    court = new
                    {
                        courtId = addedCourt.CourtId,
                        name = addedCourt.Name,
                        location = addedCourt.Location,
                        type = addedCourt.Type,
                        isPreferred = addedCourt.IsPreferred
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding court to client");
                return Json(new { success = false, message = "Error adding court to client" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCourtData(string courtId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Get court details (replace with your actual API call)
                // var court = await _clientApi.GetCourtByIdAsync(courtId, accessToken, cancellationToken);

                // For now, mock the response
                var court = new Court
                {
                    CourtId = courtId,
                    Name = "Mock Court",
                    Location = "Mock Location",
                    Type = "Indoor",
                    IsPreferred = true
                };

                return Json(court);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving court data: {CourtId}", courtId);
                return Json(new { success = false, message = "Error retrieving court data" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClientCourt([FromBody] CourtDto courtData, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Create a Court object from the DTO
                var court = new Court
                {
                    CourtId = courtData.CourtId,
                    ClientId = courtData.ClientId,
                    Name = courtData.Name,
                    Location = courtData.Location,
                    Type = courtData.Type,
                    IsPreferred = courtData.IsPreferred
                };

                // Update court (replace with your actual API call)
                // await _clientApi.UpdateClientCourtAsync(court, accessToken, cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating court: {CourtId}", courtData.CourtId);
                return Json(new { success = false, message = "Error updating court" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveClientCourt([FromBody] RemoveCourtDto removeData, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                // Remove court (replace with your actual API call)
                // await _clientApi.RemoveClientCourtAsync(removeData.ClientId, removeData.CourtId, accessToken, cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing court: {CourtId} from client: {ClientId}",
                    removeData.CourtId, removeData.ClientId);
                return Json(new { success = false, message = "Error removing court" });
            }
        }
    }

    // DTOs for the court operations
    public class CourtDto
    {
        public string CourtId { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public bool IsPreferred { get; set; }
    }

    public class RemoveCourtDto
    {
        public string ClientId { get; set; }
        public string CourtId { get; set; }
    }

    // Court model (you may already have this in your Domain project)
    public class Court
    {
        public string CourtId { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public bool IsPreferred { get; set; }
    }
}