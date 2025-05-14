using Domain;

namespace DataLayer.DAL
{
    public interface IRatingRepository : IDisposable
    {
        Task<List<Rating>> GetRatings();
        Task<Rating> GetRatingById(string RatingId);
        Task InsertRating(Rating model);
        Task DeleteRating(string RatingId); 
        Task<int> Save();

    }
}
