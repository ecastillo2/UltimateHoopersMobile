using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer.DAL
{
    public class RatingRepository : IRatingRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
       
        public RatingRepository(HUDBContext context)
        {
            this._context = context;
            
           
        }

        /// <summary>
        /// Get RatingById
        /// </summary>
        /// <param name="RatingId"></param>
        /// <returns></returns>
        public async Task<Rating> GetRatingById(string RatingId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Rating
                                       where model.RatingId == RatingId
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
        /// Get Ratings
        /// </summary>
        /// <returns></returns>
        public async Task<List<Rating>> GetRatings()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await (from rating in context.Rating
                                       select new Rating
                                       {
                                           RatingId = rating.RatingId,
                                           ProfileId = rating.ProfileId,
                                           RatedByProfileId = rating.RatedByProfileId,
                                           CreatedDate = rating.CreatedDate,
                     

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
        /// Insert Rating
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertRating(Rating model)
        {
            using (var context = _context)
            {
                try
                {
                    // Check if the profile has already been rated by RatedByProfileId
                    var existingRating = await context.Rating
                        .FirstOrDefaultAsync(r => r.ProfileId == model.ProfileId && r.RatedByProfileId == model.RatedByProfileId);

                    if (existingRating != null)
                    {
                        // If the rating exists, update the StarRating and CreatedDate
                        existingRating.StarRating = model.StarRating;
                        existingRating.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Update the timestamp
                    }
                    else
                    {
                        // If the rating does not exist, create a new rating record
                        model.RatingId = Guid.NewGuid().ToString();
                        model.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Set the timestamp

                        await context.Rating.AddAsync(model);
                    }

                    // Save changes
                    await Save();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    Console.WriteLine($"Error inserting or updating rating: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Delete Rating
        /// </summary>
        /// <param name="RatingId"></param>
        /// <returns></returns>
        public async Task DeleteRating(string RatingId)
        {
            using (var context = _context)
            {
                Rating obj = (from u in context.Rating
                              where u.RatingId == RatingId
                              select u).FirstOrDefault();



                _context.Rating.Remove(obj);
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
