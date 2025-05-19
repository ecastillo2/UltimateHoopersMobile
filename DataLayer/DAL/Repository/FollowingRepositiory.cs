using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class FollowingRepository : IFollowingRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;

        /// <summary>
        /// Following Repository
        /// </summary>
        /// <param name="context"></param>
        public FollowingRepository(HUDBContext context)
        {
            _context = context;
            
           
        }

        /// <summary>
        /// Get Following By Id
        /// </summary>
        /// <param name="FollowingId"></param>
        /// <returns></returns>
        public async Task<Following> GetFollowingById(string FollowingId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Following
                                       where model.FollowingId == FollowingId
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
        /// Get Following By Id
        /// </summary>
        /// <param name="FollowingId"></param>
        /// <returns></returns>
        public async Task UnFollow(string UnFollowingProfileId, string ProfileId)
        {
            using (var context = _context)
            {
                Following obj = (from u in context.Following
                                 where u.FollowingProfileId == UnFollowingProfileId && u.ProfileId == ProfileId
                                 select u).FirstOrDefault();



                 _context.Following.Remove(obj);
                await Save();
            }
        }


        /// <summary>
        /// Get Followings
        /// </summary>
        /// <returns></returns>
        public async Task<List<Following>> GetFollowings()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.Following
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
        /// Insert Following
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertFollowing(Following model)
        {
            using (var context = _context)
            {
                try
                {
                    model.FollowingId = Guid.NewGuid().ToString();

                    await context.Following.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }


        /// <summary>
        /// Delete Following
        /// </summary>
        /// <param name="FollowingId"></param>
        /// <returns></returns>
        public async Task DeleteFollowing(string FollowingId)
        {
            using (var context = _context)
            {
                Following obj = (from u in context.Following
                                 where u.FollowingId == FollowingId
                                 select u).FirstOrDefault();



                _context.Following.Remove(obj);
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
