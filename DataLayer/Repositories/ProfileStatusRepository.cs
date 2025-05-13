using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for ProfileStatus entity operations
    /// </summary>
    public class ProfileStatusRepository : GenericRepository<ProfileStatus>, IProfileStatusRepository
    {
        public ProfileStatusRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get profile status by profile ID
        /// </summary>
        public override async Task<ProfileStatus> GetByIdAsync(object id)
        {
            string profileId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(ps => ps.ProfileId == profileId);
        }
    }
}