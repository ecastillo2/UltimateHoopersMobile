using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Common;

namespace DataLayer.DAL
{
    public class PostCommentRepository : IPostCommentRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;

        /// <summary>
        /// PostComment Repository
        /// </summary>
        /// <param name="context"></param>
        public PostCommentRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get PostComment By Id
        /// </summary>
        /// <param name="PostCommentId"></param>
        /// <returns></returns>
        public async Task<PostComment> GetPostCommentById(string PostCommentId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.PostComment
                                       where model.PostCommentId == PostCommentId
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
        /// Get PostComments
        /// </summary>
        /// <param name="userTimeZoneId"></param>
        /// <returns></returns>
        public async Task<List<PostComment>> GetPostComments(string userTimeZoneId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.PostComment
                                       select model).ToListAsync();

                    foreach (var item in query)
                    {

                        // Convert the string to DateTime
                        DateTime dateTime = (DateTime)item.PostCommentDate;
                        // Get the current time
                        DateTime now = DateTime.Now;

                        // Calculate the difference
                        TimeSpan timeDifference = now - dateTime;

                        // Call the method to get the "ago" string
                        string result = RelativeTime.GetRelativeTime(dateTime, userTimeZoneId);

                        item.RelativeTime = result;
                       
                    }
                    query = query.OrderByDescending(post => post.PostCommentDate).ToList();
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
        /// Get PostComment By PostId
        /// </summary>
        /// <param name="PostId"></param>
        /// <param name="userTimeZoneId"></param>
        /// <returns></returns>
        public async Task<List<PostComment>> GetPostCommentByPostId(string PostId, string userTimeZoneId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to join PostComment with Profile based on PostCommentByProfileId and ProfileId
                    var query = await (from comment in context.PostComment
                                       join profile in context.Profile
                                       on comment.PostCommentByProfileId equals profile.ProfileId
                                       where comment.PostId == PostId
                                       select new PostComment
                                       {
                                           PostCommentId = comment.PostCommentId,
                                           PostId = comment.PostId,
                                           PostCommentByProfileId = comment.PostCommentByProfileId,
                                           UserComment = comment.UserComment,
                                           PostCommentDate = comment.PostCommentDate,
                                           RelativeTime = null, // This will be computed later
                                           ProfileImageURL = profile.ImageURL,
                                           UserName = profile.UserName
                                       }).ToListAsync();

                    foreach (var item in query)
                    {
                        // Convert the string to DateTime
                        DateTime dateTime = (DateTime)item.PostCommentDate;
                        // Get the current time
                        DateTime now = DateTime.Now;

                        // Calculate the difference
                        TimeSpan timeDifference = now - dateTime;

                        // Call the method to get the "ago" string
                        string result = RelativeTime.GetRelativeTime(dateTime, userTimeZoneId);

                        item.RelativeTime = result;
                    }

                    query = query.OrderByDescending(post => post.PostCommentDate).ToList();
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
        /// Insert PostComment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertPostComment(PostComment model)
        {
            using (var context = _context)
            {
                try
                {
                    model.PostCommentId = Guid.NewGuid().ToString();
                    model.PostCommentDate = DateTime.Now;

                    await context.PostComment.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Delete PostComment
        /// </summary>
        /// <param name="PostCommentId"></param>
        /// <returns></returns>
        public async Task DeletePostComment(string PostCommentId)
        {
            using (var context = _context)
            {
                PostComment obj = (from u in context.PostComment
                                   where u.PostCommentId == PostCommentId
                                   select u).FirstOrDefault();



                _context.PostComment.Remove(obj);
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
