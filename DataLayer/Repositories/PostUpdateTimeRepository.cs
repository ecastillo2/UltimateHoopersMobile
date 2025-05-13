using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for PostUpdateTime entity operations
    /// </summary>
    public class PostUpdateTimeRepository : GenericRepository<PostUpdateTime>, IPostUpdateTimeRepository
    {
        public PostUpdateTimeRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get post update time by ID
        /// </summary>
        public override async Task<PostUpdateTime> GetByIdAsync(object id)
        {
            string postUpdateTimeId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(put => put.PostUpdateTimeId == postUpdateTimeId);
        }

        /// <summary>
        /// Add new post update time
        /// </summary>
        public override async Task AddAsync(PostUpdateTime postUpdateTime)
        {
            if (string.IsNullOrEmpty(postUpdateTime.PostUpdateTimeId))
                postUpdateTime.PostUpdateTimeId = Guid.NewGuid().ToString();

            await base.AddAsync(postUpdateTime);
        }
    }
}