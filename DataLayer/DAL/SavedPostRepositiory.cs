using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;


namespace DataLayer.DAL
{
    public class SavedPostRepository : ISavedPostRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       

        public SavedPostRepository(HUDBContext context)
        {
            this._context = context;
            
           
        }

        /// <summary>
        /// GetS avedPost By Id
        /// </summary>
        /// <param name="SavedPostId"></param>
        /// <returns></returns>
        public async Task<SavedPost> GetSavedPostById(string SavedPostId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.SavedPost
                                       where model.SavedPostId == SavedPostId
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
        /// Get SavedPost By ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<List<SavedPost>> GetSavedPostByProfileId(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.SavedPost
                                       where model.SavedByProfileId == ProfileId
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
        /// Get SavedPosts
        /// </summary>
        /// <returns></returns>
        public async Task<List<SavedPost>> GetSavedPosts()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.SavedPost
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
        /// Insert SavedPost
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertSavedPost(SavedPost model)
        {
            using (var context = _context)
            {
                try
                {
                    model.SavedPostId = Guid.NewGuid().ToString();
                    model.SavedDate = DateTime.Now.ToString();

                    await context.SavedPost.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Delete SavedPost
        /// </summary>
        /// <param name="PostId"></param>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task DeleteSavedPost(string PostId, string ProfileId)
        {
            using (var context = _context)
            {
                SavedPost obj = (from u in context.SavedPost
                                 where u.PostId == PostId && u.SavedByProfileId == ProfileId
                                 select u).FirstOrDefault();



                _context.SavedPost.Remove(obj);
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
