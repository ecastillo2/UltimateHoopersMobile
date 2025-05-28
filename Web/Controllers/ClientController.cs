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
                     clientCourts = await _clientApi.GetClientCourtsAsync(id, accessToken, cancellationToken);

                   
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

                        name = c.Name
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
    }
}