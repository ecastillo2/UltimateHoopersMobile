using System.Net;
using Microsoft.AspNetCore.Mvc;
using DataLayer.DAL;
using DataLayer;
using Domain;
using Microsoft.AspNetCore.Authorization;



namespace WebAPI.Controllers
{
    /// <summary>
    /// User Controller
    /// </summary>
    [Route("api/[controller]")]
    
    public class UserController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IUserRepository userRepository;        
        private readonly IConfiguration _configuration;


        /// <summary>
        /// User Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public UserController(HUDBContext context, IConfiguration configuration)
        {
           
            this._configuration = configuration;
            this.userRepository = new UserRepository(context, configuration);

        }

        /// <summary>
        /// Get Users
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUsers")]
        [Authorize]
        public async Task<List<User>> GetUsers()
        {

            return await userRepository.GetUsers();

        }

        /// <summary>
        /// Get UserId
        /// </summary>
        /// <param name="userId"></param>
        //[Authorize]
        [HttpGet("GetUserId")]
       
        public async Task<User> GetUserById(string userId)
        {
            try
            {
                return await userRepository.GetUserById(userId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get UserId
        /// </summary>
        /// <param name="userId"></param>
        //[Authorize]
        [HttpGet("GetAdminUsers")]

        public async Task<List<User>> GetAdminUsers()
        {
            try
            {
                return await userRepository.GetAdminUsers();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="user"></param>
        [HttpPost("CreateUser")]
        public async Task CreateUser([FromBody] User user)
        {
            
            try
            {
                  await  userRepository.InsertUser(user);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteUser")]
        [Authorize]
        public async Task<HttpResponseMessage> DeleteUser(string userId)
        {
            try
            {
                await userRepository.DeleteUser(userId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteUser");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateUser")]
        public async Task UpdateUser([FromBody] User user)
        {

            try
            {
               await userRepository.UpdateUser(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update PlayerName
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdatePlayerName")]
        public async Task UpdatePlayerName([FromBody] User user)
        {

            try
            {
                await userRepository.UpdatePlayerName(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update Password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdatePassword")]
        public async Task UpdatePassword([FromBody] User user)
        {

            try
            {
                await userRepository.UpdatePassword(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update Name
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateName")]
        public async Task UpdateName([FromBody] User user)
        {

            try
            {
                await userRepository.UpdateName(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }


        /// <summary>
        /// Update User Email
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateUserEmail")]
        public async Task UpdateUserEmail([FromBody] User user)
        {

            try
            {
                await userRepository.UpdateUserEmail(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update UserName
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateUserName")]
        public async Task UpdateUserName([FromBody] User user)
        {

            try
            {
                await userRepository.UpdateUserName(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }



        /// <summary>
        /// Update UserName
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateSeg")]
        public async Task UpdateSeg([FromBody] User user)
        {

            try
            {
                await userRepository.UpdateSeg(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update UserName
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateSubId")]
        public async Task UpdateSubId([FromBody] User user)
        {

            try
            {
                await userRepository.UpdateSubId(user);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// IsEmail Available
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("IsEmailAvailable")]
       
        public async Task<bool> IsEmailAvailable(string email)
        {
            bool isAvailable;
           
            try
            {
                return  await userRepository.IsEmailAvailable(email);

            }
            catch (Exception ex)
            {
                var x = ex;
            }

            return false;
        }

        /// <summary>
        /// Get User By Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("GetUserByEmail")]
       

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                return await userRepository.GetUserByEmail(email);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Reset ForgottenPassword
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
      
        [HttpPost("ResetForgottenPassword")]
        public async Task ResetForgottenPassword([FromBody] User user)
        {

            try
            {
                await userRepository.ResetForgottenPassword(user);
            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Generate Password
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("GeneratePassword")]
        
        public async Task GeneratePassword(string userId)
        {
            try
            {
                await userRepository.GeneratePassword(userId);

               
            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }


        /// <summary>
        /// Update LastLoginDate
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("UpdateLastLoginDate")]
        public async Task UpdateLastLoginDate(string userId)
        {
            try
            {
                await userRepository.UpdateLastLoginDate(userId);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }


        /// <summary>
        /// UnActivate Account
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("UnActivateAccount")]
        [Authorize]
        public async Task UnActivateAccount(string userId)
        {
            try
            {
                await userRepository.UnActivateAccount(userId);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }
    }
}
