using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Website.ViewModels
{
    public class ReportViewModel
    {
        // Report configuration
        public List<string> AvailableReportTypes { get; set; } = new List<string>();

        public ReportDto? reportDto { get; set; }
        public IList<Run> RuntList { get; set; }
        public IList<Post> EventList { get; set; }

        [Required(ErrorMessage = "Report type is required")]
        [Display(Name = "Report Type")]
        public string SelectedReportType { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Group by option is required")]
        [Display(Name = "Group By")]
        public string GroupBy { get; set; }

        // Report data
        public List<ReportDataPoint> ReportData { get; set; } = new List<ReportDataPoint>();

        // Helper properties
        public string ReportTitle => GetReportTitle();
        public string ReportDescription => GetReportDescription();

        private string GetReportTitle()
        {
            return SelectedReportType switch
            {
                "UserActivity" => "User Activity Report",
                "ClientEngagement" => "Client Engagement Report",
                "RunParticipation" => "Run Participation Report",
                "NewSignups" => "New Signups Report",
                "UserRetention" => "User Retention Report",
                _ => "Custom Report",
            };
        }

        private string GetReportDescription()
        {
            return SelectedReportType switch
            {
                "UserActivity" => "Tracks user logins, profile views, and feature usage over time.",
                "ClientEngagement" => "Measures client sessions booked, messages sent, and goals created.",
                "RunParticipation" => "Monitors the number of runs created, total participants, and average participants per run.",
                "NewSignups" => "Tracks new account registrations and conversion rates from free to paid tiers.",
                "UserRetention" => "Measures user retention rates at different intervals (1-day, 7-day, 30-day).",
                _ => "Custom report with selected metrics.",
            };
        }
    }

    public class ReportDataPoint
    {
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public double Value { get; set; }
    }
}