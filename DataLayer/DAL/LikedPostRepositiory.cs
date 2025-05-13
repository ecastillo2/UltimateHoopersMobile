using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class LikedPostRepository : ILikedPostRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;

        public LikedPostRepository(HUDBContext context)
        {
            this._context = context;
            
           
        }

        /// <summary>
        /// Get LikedPost By Id
        /// </summary>
        /// <param name="LikedPostId"></param>
        /// <returns></returns>
        public async Task<LikedPost> GetLikedPostById(string LikedPostId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.LikedPost
                                       where model.LikedPostId == LikedPostId
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
        /// Get LikedPost By ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<List<LikedPost>> GetLikedPostByProfileId(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.LikedPost
                                       where model.LikedByProfileId == ProfileId
                                       select model).ToListAsync();

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
        /// Get LikedPosts
        /// </summary>
        /// <returns></returns>
        public async Task<List<LikedPost>> GetLikedPosts()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.LikedPost
                                       select model).ToListAsync();

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
        /// Insert LikedPost
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertLikedPost(LikedPost model)
        {
            using (var context = _context)
            {
                try
                {
                    model.LikedPostId = Guid.NewGuid().ToString();
                    model.LikedDate = DateTime.Now.ToString();

                    await context.LikedPost.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Delete LikedPost
        /// </summary>
        /// <param name="PostId"></param>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task DeleteLikedPost(string PostId, string ProfileId)
        {
            using (var context = _context)
            {
                LikedPost obj = (from u in context.LikedPost
                                 where u.PostId == PostId && u.LikedByProfileId == ProfileId
                                 select u).FirstOrDefault();

                _context.LikedPost.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
 
    }
}
