using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class FollowerRepository : IFollowerRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;

        /// <summary>
        /// Follower Repository
        /// </summary>
        /// <param name="context"></param>
        public FollowerRepository(ApplicationContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get Follower By Id
        /// </summary>
        /// <param name="FollowerId"></param>
        /// <returns></returns>
        public async Task<Follower> GetFollowerById(string FollowerId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Follower
                                       where model.FollowerId == FollowerId
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
        /// Get Followers
        /// </summary>
        /// <returns></returns>
        public async Task<List<Follower>> GetFollowers()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.Follower
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
        /// Insert Follower
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertFollower(Follower model)
        {
            using (var context = _context)
            {
                try
                {
                    model.FollowerId = Guid.NewGuid().ToString();

                    await context.Follower.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }    

        /// <summary>
        /// DeleteFollower
        /// </summary>
        /// <param name="FollowerId"></param>
        /// <returns></returns>
        public async Task DeleteFollower(string FollowerId)
        {
            using (var context = _context)
            {
                Follower obj = (from u in context.Follower
                                where u.FollowerId == FollowerId
                                select u).FirstOrDefault();



                _context.Follower.Remove(obj);
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
