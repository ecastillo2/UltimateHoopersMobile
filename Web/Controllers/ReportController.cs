using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Website.Attributes;
using Website.Services;
using Website.ViewModels;

namespace Web.Controllers
{
    [Authentication("Admin")] // Only allow administrators to access reports
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly AuthenticationService _authenticationService;

        public ReportController(
            ILogger<ReportController> logger,
            AuthenticationService authenticationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        [HttpGet]
        public IActionResult Report()
        {
            ViewData["UserType"] = _authenticationService.IsAdmin ? "Admin" : "Client";

            // Initialize the view model with default values
            var model = new ReportViewModel
            {
                AvailableReportTypes = new List<string>
                {
                    "UserActivity",
                    "ClientEngagement",
                    "RunParticipation",
                    "NewSignups",
                    "UserRetention"
                },
                SelectedReportType = "UserActivity",
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now,
                GroupBy = "Day"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                ViewData["UserType"] = _authenticationService.IsAdmin ? "Admin" : "Client";

                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to generate reports.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Validate date range
                if (model.StartDate > model.EndDate)
                {
                    ModelState.AddModelError("StartDate", "Start date cannot be after end date.");
                    return View("Index", model);
                }

                // Generate mock report data
                model.ReportData = await GenerateMockReportData(model, cancellationToken);

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["Error"] = "An error occurred while generating the report. Please try again later.";
                return View("Index", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(string reportType, DateTime startDate, DateTime endDate, string groupBy, string format, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the access token from session
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to export reports.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Create a model for generating the report data
                var model = new ReportViewModel
                {
                    SelectedReportType = reportType,
                    StartDate = startDate,
                    EndDate = endDate,
                    GroupBy = groupBy
                };

                // Generate the report data
                var reportData = await GenerateMockReportData(model, cancellationToken);

                // Export based on the requested format
                switch (format.ToLower())
                {
                    case "csv":
                        return File(System.Text.Encoding.UTF8.GetBytes(ConvertToCsv(reportData)), "text/csv", $"{reportType}_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.csv");

                    case "json":
                        return File(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(reportData)), "application/json", $"{reportType}_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.json");

                    default:
                        TempData["Error"] = "Unsupported export format.";
                        return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["Error"] = "An error occurred while exporting the report. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        private async Task<List<ReportDataPoint>> GenerateMockReportData(ReportViewModel model, CancellationToken cancellationToken)
        {
            // Simulate async operation
            await Task.Delay(500, cancellationToken);

            var reportData = new List<ReportDataPoint>();
            var random = new Random();

            // Determine the date increment based on the group by option
            TimeSpan increment = model.GroupBy switch
            {
                "Day" => TimeSpan.FromDays(1),
                "Week" => TimeSpan.FromDays(7),
                "Month" => TimeSpan.FromDays(30),
                _ => TimeSpan.FromDays(1)
            };

            // Create data points for each period in the date range
            for (DateTime date = model.StartDate; date <= model.EndDate; date += increment)
            {
                // Generate different metrics based on the report type
                switch (model.SelectedReportType)
                {
                    case "UserActivity":
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Logins",
                            Value = random.Next(50, 200)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Profile Views",
                            Value = random.Next(30, 150)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Feature Usage",
                            Value = random.Next(20, 100)
                        });
                        break;

                    case "ClientEngagement":
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Sessions Booked",
                            Value = random.Next(5, 30)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Messages Sent",
                            Value = random.Next(10, 60)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Goals Created",
                            Value = random.Next(3, 15)
                        });
                        break;

                    case "RunParticipation":
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Runs Created",
                            Value = random.Next(2, 10)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Run Participants",
                            Value = random.Next(15, 80)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Average Participants",
                            Value = random.Next(5, 12)
                        });
                        break;

                    case "NewSignups":
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Free Accounts",
                            Value = random.Next(10, 50)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Pro Accounts",
                            Value = random.Next(2, 20)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Conversion Rate",
                            Value = random.Next(5, 25)
                        });
                        break;

                    case "UserRetention":
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "1-day Retention",
                            Value = random.Next(70, 95)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "7-day Retention",
                            Value = random.Next(40, 75)
                        });
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "30-day Retention",
                            Value = random.Next(20, 60)
                        });
                        break;

                    default:
                        // Default case for other report types
                        reportData.Add(new ReportDataPoint
                        {
                            Date = date,
                            Category = "Default Metric",
                            Value = random.Next(10, 100)
                        });
                        break;
                }
            }

            return reportData;
        }

        private string ConvertToCsv(List<ReportDataPoint> data)
        {
            // Create header
            var csv = "Date,Category,Value\n";

            // Add rows
            foreach (var point in data)
            {
                csv += $"{point.Date:yyyy-MM-dd},{point.Category},{point.Value}\n";
            }

            return csv;
        }
    }
}