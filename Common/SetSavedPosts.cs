using Domain;

namespace Common
{
    public static class SetSavedPost
    {
        /// <summary>
        /// Update SavedPosts
        /// </summary>
        /// <param name="savedPostList"></param>
        /// <param name="postList"></param>
        /// <returns></returns>
        public static IList<Post> UpdateSavedPosts(IList<SavedPost> savedPostList, IList<Post> postList)
        {
            var savedPostIds = new HashSet<string>(savedPostList.Select(sp => sp.PostId));

            // Update SavedPost property based on the presence in savedPostList
            foreach (var post in postList)
            {
                post.SavedPost = savedPostIds.Contains(post.PostId);
            }

            return postList;
        }

        /// <summary>
        /// Update SavedPosts
        /// </summary>
        /// <param name="savedPostList"></param>
        /// <param name="postList"></param>
        /// <returns></returns>
        public static Post UpdateSavedPost(IList<SavedPost> savedPostList, Post post)
        {
            var savedPostIds = new HashSet<string>(savedPostList.Select(sp => sp.PostId));

           
                post.SavedPost = savedPostIds.Contains(post.PostId);
            

            return post;
        }
    }
}
