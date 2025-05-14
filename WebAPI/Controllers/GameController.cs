using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Repositories;
using System;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Game Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Game Controller Constructor
        /// </summary>
        /// <param name="repository">Game repository</param>
        /// <param name="configuration">Configuration</param>
        public GameController(IGameRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get All Games
        /// </summary>
        /// <returns>List of games</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGames()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Game By Id
        /// </summary>
        /// <param name="id">Game ID</param>
        /// <returns>Game</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(string id)
        {
            var game = await _repository.GetByIdAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            return game;
        }

        /// <summary>
        /// Get Games By Profile Id
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>List of games for the profile</returns>
        [HttpGet("profile/{profileId}")]
        public async Task<ActionResult<IEnumerable<Game>>> GetGamesByProfileId(string profileId)
        {
            return await _repository.GetGamesByProfileIdAsync(profileId);
        }

        /// <summary>
        /// Create Game
        /// </summary>
        /// <param name="game">Game to create</param>
        /// <returns>Created game</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Game>> CreateGame(Game game)
        {
            try
            {
                await _repository.AddAsync(game);
                await _repository.SaveAsync();

                return CreatedAtAction(nameof(GetGame), new { id = game.GameId }, game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the game", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Game
        /// </summary>
        /// <param name="id">Game ID</param>
        /// <param name="game">Updated game data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateGame(string id, Game game)
        {
            if (id != game.GameId)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateGameAsync(game);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Check if game exists
                var existingGame = await _repository.GetByIdAsync(id);
                if (existingGame == null)
                {
                    return NotFound();
                }

                return StatusCode(500, new { message = "An error occurred while updating the game", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Game
        /// </summary>
        /// <param name="id">Game ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteGame(string id)
        {
            var game = await _repository.GetByIdAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            try
            {
                await _repository.DeleteByIdAsync(id);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the game", error = ex.Message });
            }
        }
    }
}