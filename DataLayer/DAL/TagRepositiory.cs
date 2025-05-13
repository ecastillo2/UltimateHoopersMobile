using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class TagRepository : ITagRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public TagRepository(HUDBContext context)
        {
            this._context = context;
            
           
        }

        /// <summary>
        /// Get Tag By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Tag> GetTagById(string TagId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Tag
                                       where model.TagId == TagId
                                       select model).FirstOrDefaultAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Tags
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tag>> GetTags()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await (from tag in context.Tag
                                       select new Tag
                                       {
                                           TagId = tag.TagId,
                                           HashTag = tag.HashTag,
                                           PostsWithTag = context.Post.Count(p => p.Caption.Contains("#"+tag.HashTag)) // Count posts that contain the hashtag
                                       }).ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Insert Tag
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertTag(Tag model)
        {
            using (var context = _context)
            {
                try
                {
                    model.TagId = Guid.NewGuid().ToString();

                    await context.Tag.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// DeleteTag
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task DeleteTag(string TagId)
        {
            using (var context = _context)
            {
                Tag obj = (from u in context.Tag
                           where u.TagId == TagId
                           select u).FirstOrDefault();



                _context.Tag.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
