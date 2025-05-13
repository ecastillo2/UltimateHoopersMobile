using Domain;

namespace SocialMedia.Common
{
    public static class SetLikedPost
    {
        /// <summary>
        /// Update Liked Posts
        /// </summary>
        /// <param name="likedPostList"></param>
        /// <param name="postList"></param>
        /// <returns></returns>
        public static IList<Post> UpdateLikedPosts(IList<LikedPost> likedPostList, IList<Post> postList)
        {
            var likedPostIds = new HashSet<string>(likedPostList.Select(sp => sp.PostId));

            // Update SavedPost property based on the presence in savedPostList
            foreach (var post in postList)
            {
                post.LikedPost = likedPostIds.Contains(post.PostId);
            }

            return postList;
        }

        /// <summary>
        /// Update Liked Posts
        /// </summary>
        /// <param name="likedPostList"></param>
        /// <param name="postList"></param>
        /// <returns></returns>
        public static Post UpdateLikedPost(IList<LikedPost> likedPostList, Post post)
        {
            var likedPostIds = new HashSet<string>(likedPostList.Select(sp => sp.PostId));

            // Update SavedPost property based on the presence in savedPostList
           
                post.LikedPost = likedPostIds.Contains(post.PostId);
            

            return post;
        }
    }
}
