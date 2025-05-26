using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.ApiClients;
using Website.ViewModels;

namespace Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientApi _clientApi;
        private readonly ILogger<RunController> _logger;

        public ClientController(
            IClientApi clientApi,
            ILogger<RunController> logger)
        {
            _clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Client(
            string cursor = null,
            int limit = 10,
            string direction = "next",
            string sortBy = "StartDate",
            CancellationToken cancellationToken = default)
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
                    //PreviousCursor = result.PreviousCursor,
                    //HasMore = result.HasMore,
                    //TotalCount = result.TotalCount,
                    //CurrentLimit = limit,
                    CurrentSortBy = sortBy,
                    //UserRole = userRole,
                    //ProfileId = profileId
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
                var run = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
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
        public async Task<IActionResult> Create(Client run, CancellationToken cancellationToken = default)
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

                

                // Create new run
                var createdRun = await _clientApi.CreateClientAsync(run, accessToken, cancellationToken);

                TempData["Success"] = "Run created successfully.";
                return RedirectToAction("Details", new { id = createdRun.ClientId });
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
                var run = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
               

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
        public async Task<IActionResult> Edit(Client run, CancellationToken cancellationToken = default)
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

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
                //if (run.ProfileId != profileId && userRole != "Admin")
                //{
                //    TempData["Error"] = "You do not have permission to edit this run.";
                //    return RedirectToAction("Details", new { id = run.RunId });
                //}

                // Update run
                await _clientApi.UpdateClientAsync(run, accessToken, cancellationToken);

                TempData["Success"] = "Run updated successfully.";
                return RedirectToAction("Details", new { id = run.ClientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating run: {RunId}", run.ClientId);
                TempData["Error"] = "An error occurred while updating the run. Please try again later.";
                return View(run);
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
                var run = await _clientApi.GetClientByIdAsync(id, accessToken, cancellationToken);
                if (run == null)
                {
                    TempData["Error"] = "Run not found.";
                    return RedirectToAction("Index");
                }

                // Verify user is creator or admin
                var profileId = HttpContext.Session.GetString("ProfileId");
                var userRole = HttpContext.Session.GetString("UserRole");
               

                // Delete run
                await _clientApi.DeleteClientAsync(id, accessToken, cancellationToken);

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

       
    }

    // View model for the Runs index page
    
}