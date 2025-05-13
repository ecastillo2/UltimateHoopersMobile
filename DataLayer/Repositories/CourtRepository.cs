// DataLayer/Repositories/CourtRepository.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Court entity operations
    /// </summary>
    public class CourtRepository : GenericRepository<Court>, ICourtRepository
    {
        public CourtRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get court by ID
        /// </summary>
        public override async Task<Court> GetByIdAsync(object id)
        {
            string courtId = id.ToString();
            return await _dbSet
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }

        /// <summary>
        /// Add new court
        /// </summary>
        public override async Task AddAsync(Court court)
        {
            if (string.IsNullOrEmpty(court.CourtId))
                court.CourtId = Guid.NewGuid().ToString();

            // Set image URL using CourtId
            string fileType = ".webp";
            court.ImageURL = $"https://uhblobstorageaccount.blob.core.windows.net/courtimage/{court.CourtId}{fileType}";

            await base.AddAsync(court);
        }

        /// <summary>
        /// Update court
        /// </summary>
        public async Task UpdateCourtAsync(Court court)
        {
            var existingCourt = await GetByIdAsync(court.CourtId);
            if (existingCourt == null)
                return;

            // Update properties
            existingCourt.Name = court.Name;
            existingCourt.Latitude = court.Latitude;
            existingCourt.Longitude = court.Longitude;
            existingCourt.Address = court.Address;
            existingCourt.Status = court.Status;
            existingCourt.NumberOfCourts = court.NumberOfCourts;
            existingCourt.RentalCostPerHour = court.RentalCostPerHour;
            existingCourt.Url = court.Url;
            existingCourt.CourtSize = court.CourtSize;
            existingCourt.CourtNumber = court.CourtNumber;
            existingCourt.City = court.City;
            existingCourt.State = court.State;
            existingCourt.Zip = court.Zip;

            _dbSet.Update(existingCourt);
            await SaveAsync();
        }
    }

    /// <summary>
    /// Interface for Court repository
    /// </summary>
    public interface ICourtRepository : IGenericRepository<Court>
    {
        Task UpdateCourtAsync(Court court);
    }
}