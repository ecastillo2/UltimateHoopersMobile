using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Profile Controller
    /// </summary>
    [Route("api/[controller]")]
    
    public class ProfileController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IProfileRepository repository;
        private readonly IConfiguration _configuration;


        /// <summary>
        /// Profile Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ProfileController(HUDBContext context, IConfiguration configuration)
        {

            this._configuration = configuration;
            this.repository = new ProfileRepository(context, _configuration);

        }

        /// <summary>
        /// Get Profiles
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProfiles")]
       // [Authorize]
        public async Task<List<Profile>> GetProfiles()
        {
            try
            {
                return await repository.GetProfiles();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Following Profiles By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetFollowingProfilesByProfileId")]
        [Authorize]
        public async Task<List<Profile>> GetFollowingProfilesByProfileId(string profileId)
        {
            try
            {
                return await repository.GetFollowingProfilesByProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Follower Profiles By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetFollowerProfilesByProfileId")]
        [Authorize]
        public async Task<List<Profile>> GetFollowerProfilesByProfileId(string profileId)
        {
            try
            {
                return await repository.GetFollowerProfilesByProfileId(profileId);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Get Profile Game History
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetProfileGameHistory")]
        [Authorize]
        public async Task<List<Game>> GetProfileGameHistory(string profileId)
        {
            try
            {
                return await repository.GetProfileGameHistory(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// GetProfileById
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("GetProfileById")]
        //[Authorize]
        public async Task<Profile> GetProfileById(string profileId)
        {
            try
            {
                return await repository.GetProfileById(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// UpdateLastRunDate
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="lastRunDate"></param>
        /// <returns></returns>
        [HttpGet("UpdateLastRunDate")]
        [Authorize]
        public async Task UpdateLastRunDate(string profileId, string lastRunDate)
        {
            try
            {
                await repository.UpdateLastRunDate(profileId, lastRunDate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// UpdateWinnerPoints
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("UpdateWinnerPoints")]
        [Authorize]
        public async Task UpdateWinnerPoints(string profileId)
        {
            try
            {
                await repository.UpdateWinnerPoints(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        
        }

        /// <summary>
        /// UpdateWinnerPoints
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("UpdateSetProfileWithBestRecord")]
        //[Authorize]
        public async Task UpdateSetProfileWithBestRecord(string profileId)
        {
            try
            {
                await repository.UpdateSetProfileWithBestRecord(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        /// <summary>
        /// UpdateWinnerPoints
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [HttpGet("UpdateSetProfileWithBestRecordToFalse")]
        //[Authorize]
        public async Task UpdateSetProfileWithBestRecordToFalse(string profileId)
        {
            try
            {
                await repository.UpdateSetProfileWithBestRecordToFalse(profileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        /// <summary>
        /// UpdateProfile
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [HttpPost("UpdateProfile")]
        [Authorize]
        public async Task UpdateProfile([FromBody] Profile profile)
        {

            try
            {
                await repository.UpdateProfile(profile);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// UpdateProfileUserName
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [HttpPost("UpdateProfileUserName")]
        [Authorize]
        public async Task UpdateProfileUserName([FromBody] Profile profile)
        {

            try
            {
                await repository.UpdateProfileUserName(profile);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
  
        }

        /// <summary>
        /// UpdateSetting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        [HttpPost("UpdateSetting")]
        [Authorize]
        public async Task UpdateSetting([FromBody] Setting setting)
        {

            try
            {
                await repository.UpdateSetting(setting);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
     
        }


        /// <summary>
        /// Is UserName Available
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("IsUserNameAvailable")]
        public async Task<bool> IsUserNameAvailable(string userName)
        {
            bool isAvailable;

            try
            {
                return await repository.IsUserNameAvailable(userName);

            }
            catch (Exception ex)
            {
                var x = ex;
            }

            return false;
        }


        /// <summary>
        /// IsEmailAvailable
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("IsEmailAvailable")]
        public async Task<bool> IsEmailAvailable(string email)
        {
            bool isAvailable;

            try
            {
                return await repository.IsEmailAvailable(email);

            }
            catch (Exception ex)
            {
                var x = ex;
            }

            return false;
        }

    }
}
