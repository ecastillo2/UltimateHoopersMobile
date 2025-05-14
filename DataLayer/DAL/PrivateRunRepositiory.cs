using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using Microsoft.VisualBasic;

namespace DataLayer.DAL
{
    public class PrivateRunRepository : IPrivateRunRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;

        /// <summary>
        /// PrivateRun Repository
        /// </summary>
        /// <param name="context"></param>
        public PrivateRunRepository(HUDBContext context)
        {
            this._context = context;

        }

        /// <summary>
        /// Get PrivateRun By Id
        /// </summary>
        /// <param name="PrivateRunId"></param>
        /// <returns></returns>
        public async Task<PrivateRun> GetPrivateRunById(string PrivateRunId)
        {
            try
            {
                var query = await (from model in _context.PrivateRun
                                   where model.PrivateRunId == PrivateRunId
                                   select new
                                   {
                                       PrivateRun = model,
                                       Court = _context.Court.FirstOrDefault(c => c.CourtId == model.CourtId),
                                       Invites = (from i in _context.PrivateRunInvite
                                                  join p in _context.Profile on i.ProfileId equals p.ProfileId
                                                  join u in _context.User on p.UserId equals u.UserId
                                                  where i.PrivateRunId == model.PrivateRunId
                                                  && (i.AcceptedInvite == "Accepted" || i.AcceptedInvite == "Accepted / Pending")
                                                  select new PrivateRunInvite
                                                  {
                                                      ProfileId = i.ProfileId,
                                                      AcceptedInvite = i.AcceptedInvite,
                                                      Present = i.Present,
                                                      UserName = p.UserName,
                                                      ImageURL = p.ImageURL,
                                                      FirstName = u.FirstName,
                                                      LastName = u.LastName,
                                                      PrivateRunInviteId = i.PrivateRunInviteId,
                                                      PrivateRunId = i.PrivateRunId,
                                                      SubId = u.SubId,
                                                  }).ToList()
                                   }).FirstOrDefaultAsync();

                if (query == null) return null;

                // Map results
                var privateRun = query.PrivateRun;
                privateRun.Court = query.Court;
                privateRun.PrivateRunInviteList = query.Invites;

                // Format RunDate if necessary

                privateRun.RelativeDate = privateRun.RunDate?.ToString("dddd, MMM d", CultureInfo.InvariantCulture);



                return privateRun;
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Error retrieving PrivateRun {PrivateRunId}: {ex.Message}", ex);
                return null;
            }
        }


        /// <summary>
        /// Get profiles invited to a PrivateRun by PrivateRunId
        /// </summary>
        /// <param name="privateRunId">The ID of the private run</param>
        /// <returns>List of profiles invited to the specified private run with accepted status</returns>
        public async Task<List<Profile>> GetProfilesByPrivateRunId(string privateRunId)
        {
            using (var context = _context)
            {
                try
                {
                    // LINQ query to get profiles invited to a specific private run along with their accepted status
                    var invitedProfiles = await (from invite in context.PrivateRunInvite
                                                 join profile in context.Profile
                                                 on invite.ProfileId equals profile.ProfileId
                                                 where invite.PrivateRunId == privateRunId
                                                 select new Profile
                                                 {
                                                     ProfileId = profile.ProfileId,
                                                     UserId = profile.UserId,
                                                     UserName = profile.UserName,
                                                     Height = profile.Height,
                                                     Weight = profile.Weight,
                                                     Position = profile.Position,
                                                     Ranking = profile.Ranking,
                                                     StarRating = profile.StarRating,
                                                     QRCode = profile.QRCode,
                                                     Bio = profile.Bio,
                                                     ImageURL = profile.ImageURL,
                                                     PlayerArchetype = profile.PlayerArchetype,
                                                     City = profile.City,
                                                     PlayerNumber = profile.PlayerNumber,
                                                     InviteStatus = invite.AcceptedInvite // Adding AcceptedInvite
                                                 }).ToListAsync();

                    // Update each profile's StarRating
                    foreach (var item in invitedProfiles)
                    {
                        item.StarRating = await GetAverageStarRatingByProfileId(item.ProfileId);
                       // item.Followed = false;
                    }


                    return invitedProfiles;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get PrivateRuns with Profile details and Invite count
        /// </summary>
        /// <returns>List of PrivateRun with Profile and Invite count</returns>
        public async Task<List<PrivateRun>> GetPrivateRuns()
        {
            using (var context = _context)
            {
                try
                {
                    // Join PrivateRun, Profile, and PrivateRunInvite tables to get the required details along with the invite count
                    var query = await (from privateRun in context.PrivateRun
                                       join profile in context.Profile
                                       on privateRun.ProfileId equals profile.ProfileId
                                       // Join with PrivateRunInvite to get the count of invites for each PrivateRun
                                       join invite in context.PrivateRunInvite
                                       on privateRun.PrivateRunId equals invite.PrivateRunId into invitesGroup

                                       // Join with Court to get the court details
                                       join court in context.Court
                                       on privateRun.CourtId equals court.CourtId
                                       select new PrivateRun
                                       {
                                           PrivateRunId = privateRun.PrivateRunId,
                                           ProfileId = privateRun.ProfileId,
                                           Status = privateRun.Status,
                                           RunDate = privateRun.RunDate,
                                           Cost = privateRun.Cost,
                                           Title = privateRun.Title,
                                           Location = privateRun.Location,
                                           Description = privateRun.Description,
                                           TeamType = privateRun.TeamType,
                                           RunTime = privateRun.RunTime,
                                           EndTime = privateRun.EndTime,
                                           Type = privateRun.Type,
                                           CreatedDate = privateRun.CreatedDate,
                                           PrivateRunNumber = privateRun.PrivateRunNumber,
                                           SkillLevel = privateRun.SkillLevel,
                                           Court = court,
                                           PlayerLimit= privateRun.PlayerLimit,
                                           // Profile details
                                          // UserName = profile.UserName,
                                           //ImageURL = profile.ImageURL,

                                           // Count of invites for the current PrivateRun
                                           //InviteCount = invitesGroup.Count().ToString(),
                                            // Including profile details
                                                        UserName = (from p in context.Profile
                                                                    where p.ProfileId == privateRun.ProfileId
                                                                    select p.UserName).FirstOrDefault(),

                                           ImageURL = (from p in context.Profile
                                                       where p.ProfileId == privateRun.ProfileId
                                                       select p.ImageURL).FirstOrDefault(),

                                           // Count of invites for the current PrivateRun
                                           InviteCount = (from prInvite in context.PrivateRunInvite
                                                          where prInvite.PrivateRunId == privateRun.PrivateRunId
                                                          select prInvite).Count().ToString(),


                                     
                                       }).ToListAsync();

                    // Iterate through each private run and count the accepted invites
                    foreach (var item in query)
                    {
                        // Count the number of accepted invites for each PrivateRun
                        item.AcceptedCount = await context.PrivateRunInvite
                            .Where(prInvite => prInvite.PrivateRunId == item.PrivateRunId && prInvite.AcceptedInvite == "Accepted" || prInvite.AcceptedInvite == "Accepted / Pending")
                            .CountAsync();

                        // Count the number of accepted invites for each PrivateRun
                        item.DeclinedCount = await context.PrivateRunInvite
                            .Where(prInvite => prInvite.PrivateRunId == item.PrivateRunId && prInvite.AcceptedInvite == "Declined")
                            .CountAsync();

                        // Count the number of accepted invites for each PrivateRun
                        item.UndecidedCount = await context.PrivateRunInvite
                            .Where(prInvite => prInvite.PrivateRunId == item.PrivateRunId && prInvite.AcceptedInvite == "Undecided")
                        .CountAsync();

                        item.RelativeDate = item.RunDate?.ToString("dddd, MMM d", CultureInfo.InvariantCulture);

                    }

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    // Consider logging ex.Message or using a logging library
                    return null;
                }
            }
        }

        /// <summary>
        /// Get list of private runs that a profile is invited to by ProfileId
        /// </summary>
        /// <param name="profileId">The ID of the profile</param>
        /// <returns>List of PrivateRun objects associated with the specified profile</returns>
        public async Task<List<PrivateRun>> GetProfileInvitesByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // LINQ query to get private runs that the profile is invited to
                    var invitedPrivateRuns = await (from invite in context.PrivateRunInvite
                                                    join privateRun in context.PrivateRun
                                                    on invite.PrivateRunId equals privateRun.PrivateRunId
                                                    where invite.ProfileId == profileId
                                                    select new PrivateRun
                                                    {
                                                        PrivateRunId = privateRun.PrivateRunId,
                                                        ProfileId = privateRun.ProfileId,
                                                        Status = privateRun.Status,
                                                        RunDate = privateRun.RunDate,
                                                        Cost = privateRun.Cost,
                                                        Title = privateRun.Title,
                                                        Location = privateRun.Location,
                                                        Description = privateRun.Description,
                                                        TeamType = privateRun.TeamType,
                                                        RunTime = privateRun.RunTime,
                                                        Type = privateRun.Type,
                                                        EndTime = privateRun.EndTime,
                                                        SkillLevel = privateRun.SkillLevel,
                                                        PlayerLimit = privateRun.PlayerLimit,
                                                        CreatedDate = privateRun.CreatedDate,
                                                        PrivateRunNumber = privateRun.PrivateRunNumber,
                                                        // Including profile details
                                                        UserName = (from p in context.Profile
                                                                    where p.ProfileId == privateRun.ProfileId
                                                                    select p.UserName).FirstOrDefault(),

                                                        ImageURL = (from p in context.Profile
                                                                    where p.ProfileId == privateRun.ProfileId
                                                                    select p.ImageURL).FirstOrDefault(),

                                                        // Count of invites for the current PrivateRun
                                                        InviteCount = (from prInvite in context.PrivateRunInvite
                                                                       where prInvite.PrivateRunId == privateRun.PrivateRunId
                                                                       select prInvite).Count().ToString()
                                                    }).ToListAsync();

                    // Iterate through each private run and count the accepted invites
                    foreach (var item in invitedPrivateRuns)
                    {
                        // Count the number of accepted invites for each PrivateRun
                        item.AcceptedCount = await context.PrivateRunInvite
                            .Where(prInvite => prInvite.PrivateRunId == item.PrivateRunId && prInvite.AcceptedInvite == "Accepted" || prInvite.AcceptedInvite == "Accepted / Pending")
                            .CountAsync();

                        // Count the number of accepted invites for each PrivateRun
                        item.DeclinedCount = await context.PrivateRunInvite
                            .Where(prInvite => prInvite.PrivateRunId == item.PrivateRunId && prInvite.AcceptedInvite == "Declined")
                            .CountAsync();

                        // Count the number of accepted invites for each PrivateRun
                        item.UndecidedCount = await context.PrivateRunInvite
                            .Where(prInvite => prInvite.PrivateRunId == item.PrivateRunId && prInvite.AcceptedInvite == "Undecided")
                            .CountAsync();
                    }



                    return invitedPrivateRuns;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get PrivateRuns By ProfileId with Profile details
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns>List of PrivateRunWithProfile</returns>
        public async Task<List<PrivateRun>> GetPrivateRunsByProfileId(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Join PrivateRun and Profile tables
                    var query = await (from privateRun in context.PrivateRun
                                       join profile in context.Profile
                                       on privateRun.ProfileId equals profile.ProfileId
                                       where privateRun.ProfileId == ProfileId
                                       select new PrivateRun
                                       {
                                           PrivateRunId = privateRun.PrivateRunId,
                                           ProfileId = privateRun.ProfileId,
                                           Status = privateRun.Status,
                                           RunDate = privateRun.RunDate,
                                           Cost = privateRun.Cost,
                                           Title = privateRun.Title,
                                           Location = privateRun.Location,
                                           TeamType = privateRun.TeamType,
                                           Description = privateRun.Description,
                                           RunTime = privateRun.RunTime,
                                           EndTime = privateRun.EndTime,
                                           PlayerLimit = privateRun.PlayerLimit,
                                           Type = privateRun.Type,
                                           CreatedDate = privateRun.CreatedDate,
                                           UserName = profile.UserName,
                                           ImageURL = profile.ImageURL,
                                           SkillLevel = privateRun.SkillLevel,
                                           PrivateRunNumber = privateRun.PrivateRunNumber,
                                       }).ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    // Consider logging ex.Message or using a logging library
                    return null;
                }
            }
        }

        /// <summary>
        /// Insert PrivateRun
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertPrivateRun(PrivateRun model)
        {
            using (var context = _context)
            {
                try
                {
                    model.PrivateRunId = Guid.NewGuid().ToString();
                    model.PrivateRunNumber = UniqueIdNumber.GenerateSixDigit();
                    model.Status = "Active";

                    await context.PrivateRun.AddAsync(model);
                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Update PrivateRun
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdatePrivateRun(PrivateRun model)
        {
            using (var context = _context)
            {
                var existingPost = context.PrivateRun.Where(s => s.PrivateRunId == model.PrivateRunId).FirstOrDefault<PrivateRun>();

                if (existingPost != null)
                {
                    existingPost.Status = model.Status;
                    existingPost.RunDate = model.RunDate;
                    existingPost.Status = model.Status;
                    existingPost.Cost = model.Cost;
                    existingPost.Title = model.Title;
                    existingPost.Location = model.Location;
                    existingPost.Description = model.Description;
                    existingPost.RunTime = model.RunTime;
                    existingPost.EndTime = model.EndTime;
                    existingPost.Type = model.Type;
                    existingPost.SkillLevel = model.SkillLevel;
                    existingPost.PlayerLimit = model.PlayerLimit;
                    existingPost.CourtId = model.CourtId;
                    existingPost.TeamType = model.TeamType;
                    

                    context.PrivateRun.Update(existingPost);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Delete PrivateRun
        /// </summary>
        /// <param name="PrivateRunId"></param>
        /// <returns></returns>
        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task RemovePrivateRun(string privateRunId)
        {
            using (var context = _context)
            {
                var existingPost = context.PrivateRun.Where(s => s.PrivateRunId == privateRunId).FirstOrDefault<PrivateRun>();

                if (existingPost != null)
                {
                    existingPost.Status = "Removed";

                    context.PrivateRun.Update(existingPost);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// GetAverageStarRatingByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<string> GetAverageStarRatingByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Query the Rating table for the specified ProfileId
                    var query = await (from rating in context.Rating
                                       where rating.ProfileId == profileId
                                       select rating.StarRating).ToListAsync();

                    // Convert the StarRating from string to integer and calculate the average
                    var averageRating = query
                        .Where(r => !string.IsNullOrEmpty(r)) // Ensure we only calculate for non-null/non-empty ratings
                        .Select(r => int.Parse(r)) // Convert to integer
                        .DefaultIfEmpty(0)         // If no ratings, return 0 as default
                        .Average();

                    // Return the average as an integer
                    return averageRating.ToString();
                }
                catch (Exception ex)
                {
                    // Handle or log exception as needed
                    return "0"; // Return 0 if any exception occurs
                }
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
