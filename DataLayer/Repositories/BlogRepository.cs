using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Blog entity operations
    /// </summary>
    public class BlogRepository : GenericRepository<Blog>, IBlogRepository
    {
        public BlogRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get blog by ID
        /// </summary>
        public override async Task<Blog> GetByIdAsync(object id)
        {
            string blogId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(b => b.BlogId == blogId);
        }

        /// <summary>
        /// Add new blog
        /// </summary>
        public override async Task AddAsync(Blog blog)
        {
            if (string.IsNullOrEmpty(blog.BlogId))
                blog.BlogId = Guid.NewGuid().ToString();

            blog.CreatedDate = DateTime.Now.ToString();

            await base.AddAsync(blog);
        }
    }
}