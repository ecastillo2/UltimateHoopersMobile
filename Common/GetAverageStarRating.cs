using Domain;

namespace Common
{
    public static class GetAverageStarRating
    {
        /// <summary>
        /// Get StarRating
        /// </summary>
        /// <param name="ratingList"></param>
        /// <returns></returns>
        public static async Task<string> GetStarRating(IList<RatePlayer> ratingList)
        {

            //var postCommentList = await RatingApi.GetPostCommentByPostId(PostId, null);

            // Calculate average star rating by ProfileId as an integer
            var averageRatingsByProfile = ratingList
                .GroupBy(r => r.ProfileId)
                .Select(g => new
                {
                    ProfileId = g.Key,
                    AverageRating = (int)Math.Floor(g.Average(r => Convert.ToInt32(r.StarRating))) // Cast to int
                });

            return  averageRatingsByProfile.ToString();
        }

	}
}
