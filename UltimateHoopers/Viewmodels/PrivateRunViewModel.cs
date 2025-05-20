using Domain;
using Domain.DtoModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UltimateHoopers.ViewModels
{
    public class PrivateRunViewModel : BindableObject
    {
        // Base properties
        public string? PrivateRunId { get; set; }
        public string? CourtId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? RunDate { get; set; }
        public decimal? Cost { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Description { get; set; }
        public string? RunTime { get; set; }
        public string? EndTime { get; set; }
        public string? Type { get; set; }
        public string? CreatedDate { get; set; }
        public string? PrivateRunNumber { get; set; }
        public string? SkillLevel { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TeamType { get; set; }
        public int? PlayerLimit { get; set; }
        public string? CourtImage { get; set; }
        public string? Url { get; set; }

        public List<PrivateRunInvite>? JoinedPlayers { get; set; }

        // New method to load runs
        public async Task LoadRunsAsync()
        {
            try
            {
                // Here you would implement the logic to load private runs
                // For example, you might use a service to fetch data from an API

                // For now, we'll just add a placeholder that logs a message
                Debug.WriteLine("LoadRunsAsync called in PrivateRunViewModel");

                // In a real implementation, you would:
                // 1. Call a service to get data
                // 2. Update properties in this view model
                // 3. Notify UI of changes

                await Task.Delay(500); // Simulate network delay
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading runs: {ex.Message}");
                // Handle the error appropriately
            }
        }
    }
}