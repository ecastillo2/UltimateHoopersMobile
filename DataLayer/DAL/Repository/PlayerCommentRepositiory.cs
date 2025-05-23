using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Common;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class PlayerCommentRepository : IPlayerCommentRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;

        /// <summary>
        /// PlayerComment Repository
        /// </summary>
        /// <param name="context"></param>
        public PlayerCommentRepository(ApplicationContext context)
        {
            _context = context;
                      
        }


        /// <summary>
        /// Get PlayerComment By Id
        /// </summary>
        /// <param name="PlayerCommentId"></param>
        /// <returns></returns>
        public async Task<PlayerComment> GetPlayerCommentById(string PlayerCommentId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.PlayerComment
                                       where model.PlayerCommentId == PlayerCommentId
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
        /// Get Player Comments
        /// </summary>
        /// <returns></returns>
        public async Task<List<PlayerComment>> GetPlayerComments()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.PlayerComment
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
        /// Get PlayerComment By ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<List<PlayerComment>> GetPlayerCommentByProfileId(string ProfileId, string timeZone)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to join PlayerComment with Profile based on ProfileId
                    var query = await (from comment in context.PlayerComment
                                       join profile in context.Profile
                                       on comment.CommentedProfileId equals profile.ProfileId
                                       where comment.ProfileId == ProfileId
                                       select new PlayerComment
                                       {
                                           PlayerCommentId = comment.PlayerCommentId,
                                           ProfileId = comment.ProfileId,
                                           CommentedProfileId = comment.CommentedProfileId,
                                           Comment = comment.Comment,
                                           DateCommented = comment.DateCommented,
                                           // Include ImageURL from the Profile
                                           ImageURL = profile.ImageURL, // Assuming you want to add ImageURL here
                                           UserName = profile.UserName,  // Add CommentedProfile's UserName
                                       }).ToListAsync();

                    foreach (var item in query)
                    {
                        if (item.DateCommented.HasValue) // Ensure it's not null
                        {
                            DateTime dateTime = item.DateCommented.Value; // Extract DateTime

                            // Get the current time
                            DateTime now = DateTime.Now;

                            // Call the method to get the "ago" string
                            item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            item.RelativeTime = "Invalid Date"; // Handle null values
                        }
                    }

                    // Sort the posts by PostedDate in descending order
                    query = query.OrderByDescending(post => post.DateCommented).ToList();

                

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
        /// Insert PlayerComment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertPlayerComment(PlayerComment model)
        {
            using (var context = _context)
            {
                try
                {
                    model.PlayerCommentId = Guid.NewGuid().ToString();
                    model.DateCommented = DateTime.Now;

                    await context.PlayerComment.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }


        /// <summary>
        /// Delete Player Comment
        /// </summary>
        /// <param name="PlayerCommentId"></param>
        /// <returns></returns>
        public async Task DeletePlayerComment(string PlayerCommentId)
        {
            using (var context = _context)
            {
                PlayerComment obj = (from u in context.PlayerComment
                                     where u.PlayerCommentId == PlayerCommentId
                                     select u).FirstOrDefault();



                _context.PlayerComment.Remove(obj);
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
