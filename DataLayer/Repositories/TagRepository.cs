using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Tag entity operations
    /// </summary>
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get tag by ID
        /// </summary>
        public override async Task<Tag> GetByIdAsync(object id)
        {
            string tagId = id.ToString();
            return await _dbSet.FirstOrDefaultAsync(t => t.TagId == tagId);
        }

        /// <summary>
        /// Get popular tags
        /// </summary>
        public async Task<List<Tag>> GetPopularTagsAsync(int count = 10)
        {
            // This is a simple implementation. In a real application, you would likely
            // count tag usage and order by that.
            return await _dbSet
                .OrderByDescending(t => t.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get posts by tag ID
        /// </summary>
        public async Task<List<Post>> GetPostsByTagIdAsync(string tagId)
        {
            // This assumes a relationship exists between posts and tags
            // Modify this query based on your actual data model
            return await _context.Post
                .Where(p => p.Category == tagId || p.TagId == tagId)
                .ToListAsync();
        }

        /// <summary>
        /// Add a new tag
        /// </summary>
        public override async Task AddAsync(Tag tag)
        {
            if (string.IsNullOrEmpty(tag.TagId))
                tag.TagId = Guid.NewGuid().ToString();

            tag.CreatedDate = DateTime.Now;

            await base.AddAsync(tag);
        }
    }
}