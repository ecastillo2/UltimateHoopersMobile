using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using DataLayer.DAL.Repository;
using DataLayer.Context;
using DataLayer.DAL.Interface;

namespace WebWebAPI.Controllers
{
    /// <summary>
    /// Game Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class GameController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IGameRepository repository;        
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Game Controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public GameController(HUDBContext context, IConfiguration configuration)
        {
            this._configuration = configuration;
            this.repository = new GameRepository(context);

        }

        /// <summary>
        /// Get Games
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetGames")]
        
        public async Task<List<Game>> GetGames()
        {

            return await repository.GetGames();

        }


        /// <summary>
        /// Get Game By Id
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [HttpGet("GetGameById")]
        public async Task<Game> GetGameById(string gameId)
        {
            try
            {
                return await repository.GetGameById(gameId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Get Games By ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetGamesByProfileId")]
        public async Task<List<Game>> GetGamesByProfileId(string ProfileId)
        {
            try
            {
                return await repository.GetGamesByProfileId(ProfileId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        [HttpPost("CreateGame")]
        public async Task CreateTag([FromBody] Game game)
        {
            
            try
            {
                  await  repository.InsertGame(game);
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Update Game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        
        [HttpPost("UpdateGame")]
        public async Task UpdateGame([FromBody] Game game)
        {

            try
            {
                await repository.UpdateGame(game);

            }
            catch
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Get Game History
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        [HttpGet("GetGameHistory")]
        public async Task<List<Game>> GetGameHistory()
        {
            try
            {
                return await repository.GetGameHistory();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete Game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteGame")]
        public async Task<HttpResponseMessage> DeleteGame(string gameId)
        {
            try
            {
                await repository.DeleteGame(gameId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteGame");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }
    }
}
