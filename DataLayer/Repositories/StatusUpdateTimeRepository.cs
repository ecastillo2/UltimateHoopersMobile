using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for StatusUpdateTime entity operations
    /// </summary>
    public class StatusUpdateTimeRepository : GenericRepository<StatusUpdateTime>, IStatusUpdateTimeRepository
    {
        public StatusUpdateTimeRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get status update time by ID
        /// </summary>
        public override async Task<StatusUpdateTime> GetByIdAsync(object id)
        {
            string statusUpdateTimeId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(sut => sut.StatusUpdateTimeId == statusUpdateTimeId);
        }

        /// <summary>
        /// Add new status update time
        /// </summary>
        public override async Task AddAsync(StatusUpdateTime statusUpdateTime)
        {
            if (string.IsNullOrEmpty(statusUpdateTime.StatusUpdateTimeId))
                statusUpdateTime.StatusUpdateTimeId = Guid.NewGuid().ToString();

            await base.AddAsync(statusUpdateTime);
        }
    }
}