using Azure.Core;
using Domain;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading;
using WebAPI.ApiClients;
using Website.Attributes;
using Website.Services;
using Website.ViewModels;

namespace Web.Controllers
{
    [Authentication] // Require authentication for all actions in this controller
    public class DashboardController : Controller
    {
        private readonly IPostApi _postApi;
        private readonly IRunApi _runApi;
        private readonly IReportApi _reportApi;
        private readonly ILogger<DashboardController> _logger;
        private readonly AuthenticationService _authenticationService;

        public DashboardController(
            IRunApi runApi,
            IPostApi postApi,
            IReportApi reportApi,
            ILogger<DashboardController> logger,
            AuthenticationService authenticationService)
        {
            _logger = logger;
            _runApi = runApi ?? throw new ArgumentNullException(nameof(runApi));
            _postApi = postApi ?? throw new ArgumentNullException(nameof(postApi));
            _reportApi = reportApi ?? throw new ArgumentNullException(nameof(reportApi));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public async Task<IActionResult> DashboardAsync(CancellationToken cancellationToken = default)
        {
            // Pass user type to the view
            ViewData["UserType"] = _authenticationService.IsClient ? "Client" : "Admin";

            // Get the access token from session
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to delete a run.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            var counts = await _reportApi.StreamAllCountsAsync(accessToken, cancellationToken);
            var postList = await _postApi.GetPostsAsync("News",accessToken, cancellationToken);
            var eventPosts = postList?.Where(p => p.PostType?.Equals("News", StringComparison.OrdinalIgnoreCase) == true).Take(4).ToList() ?? new List<Post>();
            var runList = await _runApi.GetRunsWithCursorAsync(null, 6, "", "", accessToken, cancellationToken);

            var viewModel = new ReportViewModel
            {
                reportDto = counts,
                RunLists = runList,
                EventList = eventPosts,
            };

            return View(viewModel);
        }

        [Authentication("Admin")] // Only allow administrators
        public async Task<IActionResult> ClientAsync()
        {
            ViewData["UserType"] = "Admin";


            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult User()
        {
            ViewData["UserType"] = "Admin";
            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult Run()
        {
            ViewData["UserType"] = "Admin";
            return View();
        }

        [Authentication("Admin")] // Only allow administrators
        public IActionResult Reports()
        {
            ViewData["UserType"] = "Admin";
            return RedirectToAction("Index", "Reports");
        }
    }
}