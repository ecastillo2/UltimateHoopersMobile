using Domain;

namespace Common
{
    public static class SetFollowing
    {
        /// <summary>
        /// Update SetFollowing
        /// </summary>
        /// <param name="followingProfileList"></param>
        /// <param name="profileList"></param>
        /// <returns></returns>
        public static async Task<IList<Profile>> UpdateSetFollowingAsync(IList<Profile> followingProfileList, IList<Profile> profileList)
        {
            return await Task.Run(() =>
            {
                var followingProfileIds = new HashSet<string>(followingProfileList.Select(sp => sp.ProfileId));

                // Update Followed property based on the presence in followingProfileList
                foreach (var profile in profileList)
                {
                    profile.Followed = followingProfileIds.Contains(profile.ProfileId);
                }

                return profileList;
            });
        }

        /// <summary>
        /// Update SetFollowingProfile
        /// </summary>
        /// <param name="followingProfileList">List of profiles that the user is following.</param>
        /// <param name="profile">The profile object to update.</param>
        /// <returns>The updated profile with the Followed status, or null if the input profile is null.</returns>
        public static async Task<Profile> UpdateSetFollowingProfile(IList<Profile> followingProfileList, Profile profile)
        {
            if (profile == null)
            {
                return null;
            }

            // Simulating an async operation (e.g., fetching data from an API or database)
            await Task.Delay(1);

            // Create a HashSet of following profile IDs for quick lookup
            var followingProfileIds = new HashSet<string>(followingProfileList.Select(sp => sp.ProfileId));

            // Update the Followed property of the provided profile based on the presence in followingProfileList
            profile.Followed = followingProfileIds.Contains(profile.ProfileId);

            return profile;
        }

    }
}
