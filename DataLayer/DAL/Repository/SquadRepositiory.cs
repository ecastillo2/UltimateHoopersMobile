using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class SquadRepository : ISquadRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;
       

        public SquadRepository(ApplicationContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get Courts
        /// </summary>
        /// <returns></returns>
        public async Task<List<Squad>> GetSquads()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all tags and include the post count for each tag
                    var query = await context.Squad.ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        public async Task<List<SquadTeam>> GetSquadTeams()
        {
            try
            {
                // Query for the SquadTeam entries
                var squadTeamList = await _context.SquadTeam
                                                  .ToListAsync();

                // Get a list of ProfileIds for the same SquadId
                var profileIds = squadTeamList.Select(st => st.ProfileId).ToList();

                // Get all profiles corresponding to the ProfileIds in one go
                var profiles = await _context.Profile
                                             .Where(p => profileIds.Contains(p.ProfileId))
                                             .ToListAsync();

                // Get all squads with distinct SquadIds and join with Profile for owner info
                var squadsWithOwners = await (from squad in _context.Squad
                                              join profile in _context.Profile on squad.OwnerProfileId equals profile.ProfileId
                                              where squadTeamList.Select(st => st.SquadId).Contains(squad.SquadId)
                                              select new
                                              {
                                                  squad.SquadId,
                                                  squad.Name,
                                                  OwnerProfile = profile
                                              }).ToListAsync();

                // Prepare the result list with SquadTeam objects and additional data
                var result = squadTeamList.Select(squadTeam => new SquadTeam
                {
                    SquadTeamId = squadTeam.SquadTeamId,
                    SquadId = squadTeam.SquadId,
                    ProfileId = squadTeam.ProfileId,
                    RequestResponse = squadTeam.RequestResponse,
                    Name = squadsWithOwners.FirstOrDefault(s => s.SquadId == squadTeam.SquadId)?.Name,  // Assign squad name to the SquadTeam
                    Owner = squadsWithOwners.FirstOrDefault(s => s.SquadId == squadTeam.SquadId)?.OwnerProfile,  // Fetch owner profile for the squad
                    ProfileList = profiles.Where(p => squadTeamList.Any(st => st.ProfileId == p.ProfileId && st.SquadId == squadTeam.SquadId)).ToList() // Map profiles to the SquadTeam based on SquadId
                })
                .OrderBy(st => st.SquadId)  // Sort by SquadId
                .ToList();

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging framework instead of Console.WriteLine)
                Console.WriteLine($"Error: {ex.Message}");
                return new List<SquadTeam>(); // Return an empty list instead of null
            }
        }








        /// <summary>
        /// Get Contact By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Squad> GetSquadById(string SquadId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.Squad
                                       where model.SquadId == SquadId
                                       select model).FirstOrDefaultAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        public async Task<List<SquadTeam>> GetPendingRequestsProfileById(string profileId)
        {
            try
            {
                // Perform the join to get the Squad, SquadTeam, Profile, and the owner's Profile data
                var query = from squadTeam in _context.SquadTeam
                            join squad in _context.Squad on squadTeam.SquadId equals squad.SquadId
                            join profile in _context.Profile on squadTeam.ProfileId equals profile.ProfileId
                            join ownerProfile in _context.Profile on squad.OwnerProfileId equals ownerProfile.ProfileId  // Join for the owner's profile
                            where squadTeam.RequestResponse == false && squadTeam.ProfileId == profileId   // Ensure that we are matching the owner's profile ID
                            select new SquadTeam
                            {
                                SquadTeamId = squadTeam.SquadTeamId,
                                SquadId = squadTeam.SquadId,
                                ProfileId = squadTeam.ProfileId,
                                RequestResponse = squadTeam.RequestResponse,
                                Name = squad.Name,  // Assign squad name to the SquadTeam
                                Owner = ownerProfile, // Assign the owner's Profile object to Owner
                            };

                // Execute the query and return the results as a list
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging framework instead of returning null)
                Console.WriteLine($"Error: {ex.Message}");
                return new List<SquadTeam>(); // Return an empty list instead of null
            }
        }


     

        public async Task<string> AddPlayerToSquad(string ProfileId, string SquadId)
        {
            using (var context = _context)
            {
                try
                {
                    // Check if the player is already in any squad
                    bool playerExists = await context.SquadTeam
                        .AnyAsync(s => s.ProfileId == ProfileId);

                    if (playerExists)
                    {
                        return "Player is already on a squad. Players cannot be in more than one squad. Remove the player from the previous squad first.";
                    }

                    // Check if the squad already has 5 players
                    int squadPlayerCount = await context.SquadTeam
                        .CountAsync(s => s.SquadId == SquadId);

                    if (squadPlayerCount >= 5)
                    {
                        return "This squad already has 5 players. You cannot add more players. Remove a player to add player";
                    }

                    // Create a new squad team entry
                    SquadTeam squadTeam = new SquadTeam
                    {
                        SquadTeamId = Guid.NewGuid().ToString(),
                        ProfileId = ProfileId,
                        SquadId = SquadId,
                        RequestResponse = false
                    };

                    await context.SquadTeam.AddAsync(squadTeam);
                    await Save();

                    return "Player added successfully.";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }


        public async Task<string> RemovePlayerFromSquad(string ProfileId, string SquadId)
        {
            using (var context = _context)
            {
                try
                {
                    // Find the player in the squad
                    var squadPlayer = await context.SquadTeam
                        .FirstOrDefaultAsync(s => s.ProfileId == ProfileId && s.SquadId == SquadId);

                    // If player is not found, return a message
                    if (squadPlayer == null)
                    {
                        return "Player not found in this squad.";
                    }

                    // Remove the player from the squad
                    context.SquadTeam.Remove(squadPlayer);
                    await Save();

                    return "Player removed successfully.";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Insert Tag
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SendPlayerRequestToJoinSquad(string ToProfileId,string FromProfileId, string SquadId)
        {
            using (var context = _context)
            {
                try
                {
                    SquadRequest squadTeamRequest = new SquadRequest();
                    squadTeamRequest.ToProfileId = ToProfileId;
                    squadTeamRequest.FromProfileId = FromProfileId;
                    squadTeamRequest.SquadId = SquadId;
                    squadTeamRequest.CreatedDate = DateTime.Now;

                    await context.SquadRequest.AddAsync(squadTeamRequest);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Delete Contact
        /// </summary>
        /// <param name="ContactId"></param>
        /// <returns></returns>
        public async Task ClearSquad(string SquadId)
        {
            using (var context = _context)
            {
                Squad obj = (from u in context.Squad
                             where u.SquadId == SquadId
                             select u).FirstOrDefault();



                _context.Squad.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        public Task<Squad> GetSquadByOwnerProfileId(string profileId)
        {
            throw new NotImplementedException();
        }

        public Task SendPlayerRequestToJoinSquad(string ProfileId, string SquadId)
        {
            throw new NotImplementedException();
        }

        
    }
}
