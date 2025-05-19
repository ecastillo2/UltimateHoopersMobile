using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Messages;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class PrivateRunInviteRepository : IPrivateRunInviteRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private HUDBContext _context;
        private EmailMessages _emailMessages;

        /// <summary>
        /// PrivateRun Repository
        /// </summary>
        /// <param name="context"></param>
        public PrivateRunInviteRepository(HUDBContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Get PrivateRun Invite By Id
        /// </summary>
        /// <param name="PrivateRunId"></param>
        /// <returns></returns>
        public async Task<PrivateRunInvite> GetPrivateRunInviteById(string PrivateRunInviteId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.PrivateRunInvite
                                       where model.PrivateRunInviteId == PrivateRunInviteId
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
        public async Task<List<PrivateRunInvite>> GetPrivateRunInvites()
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to select all posts
                    var query = await (from model in context.PrivateRunInvite
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
        public async Task<List<PrivateRunInvite>> GetPrivateRunInvitesByProfileId(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to query for the Post with the matching PostId
                    var query = await (from model in context.PrivateRunInvite
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
        public async Task<bool> IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites(string profileId, string PrivateRunId)
        {
            using (var context = _context)
            {
                try
                {
                    bool item = (from u in context.PrivateRunInvite
                                 where u.ProfileId == profileId && u.PrivateRunId == PrivateRunId
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
        public async Task UpdatePlayerPrivateRunInvite(string ProfileId, string PrivateRunInviteId, string AcceptedInvite)
        {
            using (var context = _context)
            {
                var existingItem = context.PrivateRunInvite.Where(s => s.ProfileId == ProfileId && s.PrivateRunInviteId == PrivateRunInviteId).FirstOrDefault<PrivateRunInvite>();

                if (existingItem != null)
                {
                    existingItem.AcceptedInvite = AcceptedInvite;
                    

                    context.PrivateRunInvite.Update(existingItem);
                    await Save();
                }
                else
                {

                }

                var existingOrder = context.Order.Where(s => s.ProfileId == ProfileId && s.PrivateRunInviteId == existingItem.PrivateRunInviteId).FirstOrDefault<Order>();

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
        public async Task UpdatePlayerPresentPrivateRunInvite(string ProfileId, string PrivateRunInviteId, bool preseent)
        {
            using (var context = _context)
            {
                var existingItem = context.PrivateRunInvite.Where(s => s.ProfileId == ProfileId && s.PrivateRunInviteId == PrivateRunInviteId).FirstOrDefault<PrivateRunInvite>();

                if (existingItem != null)
                {
                    existingItem.Present = preseent;


                    context.PrivateRunInvite.Update(existingItem);
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
        public async Task InsertPrivateRunInvite(PrivateRunInvite model)
        {
            using (var context = _context)
            {
                try
                {
                    
                    model.InvitedDate = DateTime.Now.ToString();
                    model.Present = false;

                    await context.PrivateRunInvite.AddAsync(model);

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
        public async Task DeletePrivateRunInvite(string PrivateRunInviteId)
        {
            using (var context = _context)
            {
                PrivateRunInvite obj = (from u in context.PrivateRunInvite
                                  where u.PrivateRunInviteId == PrivateRunInviteId
                                  select u).FirstOrDefault();



                _context.PrivateRunInvite.Remove(obj);
                await Save();
            }
        }


        /// <summary>
        /// Delete PrivateRun Invite
        /// </summary>
        /// <param name="PrivateRunInviteId"></param>
        /// <returns></returns>
        public async Task ClearPrivateRunInviteByPrivateRun(string PrivateRunId)
        {
            using (var context = _context)
            {
                // Select all records with PrivateRunId == 7
                var recordsToDelete = from u in context.PrivateRunInvite
                                      where u.PrivateRunId == PrivateRunId
                                      select u;

                // Remove all selected records
                context.PrivateRunInvite.RemoveRange(recordsToDelete);

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
        public async Task<bool> RemoveProfileFromPrivateRun(string profileId, string privateRunId)
        {
            var obj = await _context.PrivateRunInvite
        .FirstOrDefaultAsync(u => u.ProfileId == profileId && u.PrivateRunId == privateRunId);

            if (obj == null)
            {
                return false; // No matching record found, return failure
            }

            _context.PrivateRunInvite.Remove(obj);
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
         

    }
}
