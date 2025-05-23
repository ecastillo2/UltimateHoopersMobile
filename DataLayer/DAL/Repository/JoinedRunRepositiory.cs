using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Messages;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class JoinedRunRepository : IJoinedRunRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;
        private EmailMessages _emailMessages;

        /// <summary>
        /// PrivateRun Repository
        /// </summary>
        /// <param name="context"></param>
        public JoinedRunRepository(ApplicationContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get PrivateRun Invite By Id
        /// </summary>
        /// <param name="PrivateRunId"></param>
        /// <returns></returns>
        public async Task<JoinedRun> GetJoinedRunById(string JoinedRunId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.JoinedRun
                                       where model.JoinedRunId == JoinedRunId
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

        /// <summary>
        /// Get PrivateRun Invites
        /// </summary>
        /// <returns></returns>
        public async Task<List<JoinedRun>> GetJoinedRuns()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.JoinedRun
                                       select model).ToListAsync();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get PrivateRuns By ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<List<JoinedRun>> GetJoinedRunsByProfileId(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.JoinedRun
                                       where model.ProfileId == ProfileId
                                       select model).ToListAsync();

                 

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }


        /// <summary>
        /// Is Email Available
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(string profileId, string RunId)
        {
            using (var context = _context)
            {
                try
                {
                    bool item = (from u in context.JoinedRun
                                 where u.ProfileId == profileId && u.RunId == RunId
                                 select u).Any();

                    return item;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return false;
                }
            }
        }

        /// <summary>
        /// Update Player PrivateRun Invite
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <param name="PrivateRunId"></param>
        /// <param name="AcceptedInvite"></param>
        /// <returns></returns>
        public async Task UpdatePlayerJoinedRun(string ProfileId, string JoinedRunId, string AcceptedInvite)
        {
            using (var context = _context)
            {
                var existingItem = context.JoinedRun.Where(s => s.ProfileId == ProfileId && s.JoinedRunId == JoinedRunId).FirstOrDefault<JoinedRun>();

                if (existingItem != null)
                {
                    existingItem.AcceptedInvite = AcceptedInvite;
                    

                    context.JoinedRun.Update(existingItem);
                    await Save();
                }
                else
                {

                }

                var existingOrder = context.Order.Where(s => s.ProfileId == ProfileId && s.JoinedRunId == existingItem.JoinedRunId).FirstOrDefault<Order>();

                if (existingOrder != null)
                {

                    if(AcceptedInvite == "Accepted")
                    {
                        existingOrder.Status = "Completed";
                    }

                    if (AcceptedInvite == "Accepted / Pending")
                    {
                        existingOrder.Status = "Pending";
                    }

                    if (AcceptedInvite == "Refund")
                    {
                        existingOrder.Status = "Refund";
                    }

                    context.Order.Update(existingOrder);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Player PrivateRun Invite
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <param name="PrivateRunId"></param>
        /// <param name="AcceptedInvite"></param>
        /// <returns></returns>
        public async Task UpdatePlayerPresentJoinedRun(string ProfileId, string JoinedRunId, bool preseent)
        {
            using (var context = _context)
            {
                var existingItem = context.JoinedRun.Where(s => s.ProfileId == ProfileId && s.JoinedRunId == JoinedRunId).FirstOrDefault<JoinedRun>();

                if (existingItem != null)
                {
                    existingItem.Present = preseent;


                    context.JoinedRun.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }


        /// <summary>
        /// Insert PrivateRun Invite
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertJoinedRun(JoinedRun model)
        {
            using (var context = _context)
            {
                try
                {
                    
                    model.InvitedDate = DateTime.Now.ToString();
                    model.Present = false;

                    await context.JoinedRun.AddAsync(model);

                }
                catch (Exception ex)
                {

                }
                await Save();

            }
        }

        /// <summary>
        /// Delete PrivateRun Invite
        /// </summary>
        /// <param name="PrivateRunInviteId"></param>
        /// <returns></returns>
        public async Task DeleteJoinedRun(string JoinedRunId)
        {
            using (var context = _context)
            {
                JoinedRun obj = (from u in context.JoinedRun
                                 where u.JoinedRunId == JoinedRunId
                                 select u).FirstOrDefault();



                _context.JoinedRun.Remove(obj);
                await Save();
            }
        }


        /// <summary>
        /// Delete PrivateRun Invite
        /// </summary>
        /// <param name="PrivateRunInviteId"></param>
        /// <returns></returns>
        public async Task ClearRunInviteByRun(string RunId)
        {
            using (var context = _context)
            {
                // Select all records with PrivateRunId == 7
                var recordsToDelete = from u in context.JoinedRun
                                      where u.RunId == RunId
                                      select u;

                // Remove all selected records
                context.JoinedRun.RemoveRange(recordsToDelete);

                // Save changes asynchronously
                await Save();
            }
        }


        /// <summary>
        /// Remove Profile From PrivateRun
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="privateRunId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveProfileFromRun(string profileId, string runId)
        {
            var obj = await _context.JoinedRun
        .FirstOrDefaultAsync(u => u.ProfileId == profileId && u.RunId == runId);

            if (obj == null)
            {
                return false; // No matching record found, return failure
            }

            _context.JoinedRun.Remove(obj);
            await Save(); // Ensure the deletion is saved

            return true; // Successfully removed
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

        public Task ClearJoinedRunByRun(string RunId)
        {
            throw new NotImplementedException();
        }
    }
}
