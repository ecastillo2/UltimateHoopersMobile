using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for PrivateRunInvite entity operations
    /// </summary>
    public class PrivateRunInviteRepository : GenericRepository<PrivateRunInvite>, IPrivateRunInviteRepository
    {
        public PrivateRunInviteRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get private run invite by ID
        /// </summary>
        public override async Task<PrivateRunInvite> GetByIdAsync(object id)
        {
            string privateRunInviteId = id.ToString();
            return await _dbSet
                .FirstOrDefaultAsync(pri => pri.PrivateRunInviteId == privateRunInviteId);
        }

        /// <summary>
        /// Get private run invites by profile ID
        /// </summary>
        public async Task<List<PrivateRunInvite>> GetByProfileIdAsync(string profileId)
        {
            return await _dbSet
                .Where(pri => pri.ProfileId == profileId)
                .ToListAsync();
        }

        /// <summary>
        /// Check if profile is already invited to run
        /// </summary>
        public async Task<bool> IsProfileAlreadyInvitedAsync(string profileId, string privateRunId)
        {
            return await _dbSet
                .AnyAsync(pri => pri.ProfileId == profileId && pri.PrivateRunId == privateRunId);
        }

        /// <summary>
        /// Update player private run invite status
        /// </summary>
        public async Task UpdateInviteStatusAsync(string profileId, string privateRunInviteId, string acceptedInvite)
        {
            var invite = await _dbSet
                .FirstOrDefaultAsync(pri => pri.ProfileId == profileId && pri.PrivateRunInviteId == privateRunInviteId);

            if (invite == null)
                return;

            invite.AcceptedInvite = acceptedInvite;

            _dbSet.Update(invite);
            await SaveAsync();

            // Update related order if it exists
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.ProfileId == profileId && o.PrivateRunInviteId == privateRunInviteId);

            if (order != null)
            {
                switch (acceptedInvite)
                {
                    case "Accepted":
                        order.Status = "Completed";
                        break;
                    case "Accepted / Pending":
                        order.Status = "Pending";
                        break;
                    case "Refund":
                        order.Status = "Refund";
                        break;
                }

                _context.Orders.Update(order);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Update player present status
        /// </summary>
        public async Task UpdatePresentStatusAsync(string profileId, string privateRunInviteId, bool present)
        {
            var invite = await _dbSet
                .FirstOrDefaultAsync(pri => pri.ProfileId == profileId && pri.PrivateRunInviteId == privateRunInviteId);

            if (invite == null)
                return;

            invite.Present = present;

            _dbSet.Update(invite);
            await SaveAsync();
        }

        /// <summary>
        /// Insert private run invite
        /// </summary>
        public override async Task AddAsync(PrivateRunInvite invite)
        {
            if (string.IsNullOrEmpty(invite.PrivateRunInviteId))
                invite.PrivateRunInviteId = Guid.NewGuid().ToString();

            invite.InvitedDate = DateTime.Now.ToString();
            invite.Present = false;

            await base.AddAsync(invite);
        }

        /// <summary>
        /// Delete all invites for a private run
        /// </summary>
        public async Task ClearInvitesByPrivateRunIdAsync(string privateRunId)
        {
            var invites = await _dbSet
                .Where(pri => pri.PrivateRunId == privateRunId)
                .ToListAsync();

            if (invites.Any())
            {
                _dbSet.RemoveRange(invites);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Remove profile from private run
        /// </summary>
        public async Task<bool> RemoveProfileFromPrivateRunAsync(string profileId, string privateRunId)
        {
            var invite = await _dbSet
                .FirstOrDefaultAsync(pri => pri.ProfileId == profileId && pri.PrivateRunId == privateRunId);

            if (invite == null)
                return false;

            _dbSet.Remove(invite);
            await SaveAsync();

            return true;
        }
    }

    /// <summary>
    /// Interface for PrivateRunInvite repository
    /// </summary>
    public interface IPrivateRunInviteRepository : IGenericRepository<PrivateRunInvite>
    {
        Task<List<PrivateRunInvite>> GetByProfileIdAsync(string profileId);
        Task<bool> IsProfileAlreadyInvitedAsync(string profileId, string privateRunId);
        Task UpdateInviteStatusAsync(string profileId, string privateRunInviteId, string acceptedInvite);
        Task UpdatePresentStatusAsync(string profileId, string privateRunInviteId, bool present);
        Task ClearInvitesByPrivateRunIdAsync(string privateRunId);
        Task<bool> RemoveProfileFromPrivateRunAsync(string profileId, string privateRunId);
    }
}