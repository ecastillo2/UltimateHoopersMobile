using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace UltimateHoopers.Services
{
    public interface IProfileService
    {
        Task<List<Profile>> GetProfilesAsync();
        Task<List<Profile>> GetProfilesWithCursor();

        Task<Profile> GetProfileByIdAsync(string postId);
        Task<bool> UpdateProfileAsync(Profile profile);
     
    }
}