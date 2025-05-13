// DataLayer/Repositories/PrivateRunRepository.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Common;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for PrivateRun entity operations
    /// </summary>
    public class PrivateRunRepository : GenericRepository<PrivateRun>, IPrivateRunRepository
    {
        public PrivateRunRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get PrivateRun by ID with all related data
        /// </summary>
        public override async Task<PrivateRun> GetByIdAsync(object id)
        {
            string privateRunId = id.ToString();
            var privateRun = await _dbSet
                .FirstOrDefaultAsync(pr => pr.PrivateRunId == privateRunId);

            if (privateRun == null)
                return null;

            // Load court information
            privateRun.Court = await _context.Courts
                .FirstOrDefaultAsync(c => c.CourtId == privateRun.CourtId);

            // Load invites with profile information
            privateRun.PrivateRunInviteList = await _context.PrivateRunInvites
                .Where(i => i.PrivateRunId == privateRunId &&
                           (i.AcceptedInvite == "Accepted" || i.AcceptedInvite == "Accepted / Pending"))
                .Join(_context.Profiles,
                    invite => invite.ProfileId,
                    profile => profile.ProfileId,
                    (invite, profile) => new PrivateRunInvite
                    {
                        ProfileId = invite.ProfileId,
                        AcceptedInvite = invite.AcceptedInvite,
                        Present = invite.Present,
                        UserName = profile.UserName,
                        ImageURL = profile.ImageURL,
                        PrivateRunInviteId = invite.PrivateRunInviteId,
                        PrivateRunId = invite.PrivateRunId
                    })
                .ToListAsync();

            // Add user info to invites
            if (privateRun.PrivateRunInviteList?.Any() == true)
            {
                var profileIds = privateRun.PrivateRunInviteList.Select(i => i.ProfileId).ToList();
                var users = await _context.Users
                    .Join(_context.Profiles,
                        user => user.UserId,
                        profile => profile.UserId,
                        (user, profile) => new {
                            ProfileId = profile.ProfileId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            SubId = user.SubId
                        })
                    .Where(u => profileIds.Contains(u.ProfileId))
                    .ToDictionaryAsync(u => u.ProfileId);

                foreach (var invite in privateRun.PrivateRunInviteList)
                {
                    if (users.TryGetValue(invite.ProfileId, out var user))
                    {
                        invite.FirstName = user.FirstName;
                        invite.LastName = user.LastName;
                        invite.SubId = user.SubId;
                    }
                }
            }

            // Format date if available
            privateRun.RelativeDate = privateRun.RunDate?.ToString("dddd, MMM d", CultureInfo.InvariantCulture);

            return privateRun;
        }

        /// <summary>
        /// Get all PrivateRuns with counts and details
        /// </summary>
        public override async Task<List<PrivateRun>> GetAllAsync()
        {
            var privateRuns = await _dbSet
                .Join(_context.Profiles,
                    pr => pr.ProfileId,
                    profile => profile.ProfileId,
                    (pr, profile) => new PrivateRun
                    {
                        PrivateRunId = pr.PrivateRunId,
                        ProfileId = pr.ProfileId,
                        Status = pr.Status,
                        RunDate = pr.RunDate,
                        Cost = pr.Cost,
                        Title = pr.Title,
                        Location = pr.Location,
                        Description = pr.Description,
                        TeamType = pr.TeamType,
                        RunTime = pr.RunTime,
                        EndTime = pr.EndTime,
                        Type = pr.Type,
                        CreatedDate = pr.CreatedDate,
                        PrivateRunNumber = pr.PrivateRunNumber,
                        SkillLevel = pr.SkillLevel,
                        PlayerLimit = pr.PlayerLimit,
                        CourtId = pr.CourtId,
                        UserName = profile.UserName,
                        ImageURL = profile.ImageURL
                    })
                .ToListAsync();

            // Get all privateRunIds
            var privateRunIds = privateRuns.Select(pr => pr.PrivateRunId).ToList();

            // Load counts in batch
            var inviteCounts = await _context.PrivateRunInvites
                .Where(pri => privateRunIds.Contains(pri.PrivateRunId))
                .GroupBy(pri => pri.PrivateRunId)
                .Select(g => new { PrivateRunId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PrivateRunId, g => g.Count);

            var acceptedCounts = await _context.PrivateRunInvites
                .Where(pri => privateRunIds.Contains(pri.PrivateRunId) &&
                             (pri.AcceptedInvite == "Accepted" || pri.AcceptedInvite == "Accepted / Pending"))
                .GroupBy(pri => pri.PrivateRunId)
                .Select(g => new { PrivateRunId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PrivateRunId, g => g.Count);

            var declinedCounts = await _context.PrivateRunInvites
                .Where(pri => privateRunIds.Contains(pri.PrivateRunId) && pri.AcceptedInvite == "Declined")
                .GroupBy(pri => pri.PrivateRunId)
                .Select(g => new { PrivateRunId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PrivateRunId, g => g.Count);

            var undecidedCounts = await _context.PrivateRunInvites
                .Where(pri => privateRunIds.Contains(pri.PrivateRunId) && pri.AcceptedInvite == "Undecided")
                .GroupBy(pri => pri.PrivateRunId)
                .Select(g => new { PrivateRunId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PrivateRunId, g => g.Count);

            // Load court data
            var courtIds = privateRuns.Where(pr => !string.IsNullOrEmpty(pr.CourtId))
                                     .Select(pr => pr.CourtId)
                                     .Distinct()
                                     .ToList();

            var courts = await _context.Courts
                .Where(c => courtIds.Contains(c.CourtId))
                .ToDictionaryAsync(c => c.CourtId);

            // Apply counts and court data to private runs
            foreach (var privateRun in privateRuns)
            {
                privateRun.InviteCount = inviteCounts.TryGetValue(privateRun.PrivateRunId, out var count)
                    ? count.ToString() : "0";

                privateRun.AcceptedCount = acceptedCounts.TryGetValue(privateRun.PrivateRunId, out var acceptedCount)
                    ? acceptedCount : 0;

                privateRun.DeclinedCount = declinedCounts.TryGetValue(privateRun.PrivateRunId, out var declinedCount)
                    ? declinedCount : 0;

                privateRun.UndecidedCount = undecidedCounts.TryGetValue(privateRun.PrivateRunId, out var undecidedCount)
                    ? undecidedCount : 0;

                // Set court data
                if (!string.IsNullOrEmpty(privateRun.CourtId) && courts.TryGetValue(privateRun.CourtId, out var court))
                {
                    privateRun.Court = court;
                }

                // Format relative date
                privateRun.RelativeDate = privateRun.RunDate?.ToString("dddd, MMM d", CultureInfo.InvariantCulture);
            }

            return privateRuns;
        }

        /// <summary>
        /// Get PrivateRuns by profile ID
        /// </summary>
        public async Task<List<PrivateRun>> GetPrivateRunsByProfileIdAsync(string profileId)
        {
            return await _dbSet
                .Where(pr => pr.ProfileId == profileId)
                .Join(_context.Profiles,
                    pr => pr.ProfileId,
                    profile => profile.ProfileId,
                    (pr, profile) => new PrivateRun
                    {
                        PrivateRunId = pr.PrivateRunId,
                        ProfileId = pr.ProfileId,
                        Status = pr.Status,
                        RunDate = pr.RunDate,
                        Cost = pr.Cost,
                        Title = pr.Title,
                        Location = pr.Location,
                        Description = pr.Description,
                        TeamType = pr.TeamType,
                        RunTime = pr.RunTime,
                        EndTime = pr.EndTime,
                        Type = pr.Type,
                        SkillLevel = pr.SkillLevel,
                        PlayerLimit = pr.PlayerLimit,
                        CourtId = pr.CourtId,
                        CreatedDate = pr.CreatedDate,
                        PrivateRunNumber = pr.PrivateRunNumber,
                        UserName = profile.UserName,
                        ImageURL = profile.ImageURL
                    })
                .ToListAsync();
        }

        /// <summary>
        /// Get profile invites by profile ID
        /// </summary>
        public async Task<List<PrivateRun>> GetProfileInvitesByProfileIdAsync(string profileId)
        {
            return await _context.PrivateRunInvites
                .Where(pri => pri.ProfileId == profileId)
                .Join(_dbSet,
                    pri => pri.PrivateRunId,
                    pr => pr.PrivateRunId,
                    (pri, pr) => new PrivateRun
                    {
                        PrivateRunId = pr.PrivateRunId,
                        ProfileId = pr.ProfileId,
                        Status = pr.Status,
                        RunDate = pr.RunDate,
                        Cost = pr.Cost,
                        Title = pr.Title,
                        Location = pr.Location,
                        Description = pr.Description,
                        TeamType = pr.TeamType,
                        RunTime = pr.RunTime,
                        EndTime = pr.EndTime,
                        Type = pr.Type,
                        SkillLevel = pr.SkillLevel,
                        PlayerLimit = pr.PlayerLimit,
                        CourtId = pr.CourtId,
                        CreatedDate = pr.CreatedDate,
                        PrivateRunNumber = pr.PrivateRunNumber
                    })
                .ToListAsync();
        }

        /// <summary>
        /// Get profiles by PrivateRun ID
        /// </summary>
        public async Task<List<Profile>> GetProfilesByPrivateRunIdAsync(string privateRunId)
        {
            return await _context.PrivateRunInvites
                .Where(pri => pri.PrivateRunId == privateRunId)
                .Join(_context.Profiles,
                    pri => pri.ProfileId,
                    profile => profile.ProfileId,
                    (pri, profile) => new Profile
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
                        InviteStatus = pri.AcceptedInvite // Adding AcceptedInvite status
                    })
                .ToListAsync();
        }

        /// <summary>
        /// Insert private run
        /// </summary>
        public override async Task AddAsync(PrivateRun privateRun)
        {
            if (string.IsNullOrEmpty(privateRun.PrivateRunId))
                privateRun.PrivateRunId = Guid.NewGuid().ToString();

            privateRun.PrivateRunNumber = UniqueIdNumber.GenerateSixDigit();
            privateRun.Status = "Active";

            await base.AddAsync(privateRun);
        }

        /// <summary>
        /// Update private run
        /// </summary>
        public async Task UpdatePrivateRunAsync(PrivateRun privateRun)
        {
            var existingPrivateRun = await GetByIdAsync(privateRun.PrivateRunId);
            if (existingPrivateRun == null)
                return;

            // Update properties
            existingPrivateRun.Status = privateRun.Status;
            existingPrivateRun.RunDate = privateRun.RunDate;
            existingPrivateRun.Cost = privateRun.Cost;
            existingPrivateRun.Title = privateRun.Title;
            existingPrivateRun.Location = privateRun.Location;
            existingPrivateRun.Description = privateRun.Description;
            existingPrivateRun.RunTime = privateRun.RunTime;
            existingPrivateRun.EndTime = privateRun.EndTime;
            existingPrivateRun.Type = privateRun.Type;
            existingPrivateRun.SkillLevel = privateRun.SkillLevel;
            existingPrivateRun.PlayerLimit = privateRun.PlayerLimit;
            existingPrivateRun.CourtId = privateRun.CourtId;
            existingPrivateRun.TeamType = privateRun.TeamType;

            _dbSet.Update(existingPrivateRun);
            await SaveAsync();
        }

        /// <summary>
        /// Remove private run (soft delete by setting status to "Removed")
        /// </summary>
        public async Task RemovePrivateRunAsync(string privateRunId)
        {
            var privateRun = await GetByIdAsync(privateRunId);
            if (privateRun == null)
                return;

            privateRun.Status = "Removed";

            _dbSet.Update(privateRun);
            await SaveAsync();
        }
    }

    /// <summary>
    /// Interface for PrivateRun repository
    /// </summary>
    public interface IPrivateRunRepository : IGenericRepository<PrivateRun>
    {
        Task<List<PrivateRun>> GetPrivateRunsByProfileIdAsync(string profileId);
        Task<List<PrivateRun>> GetProfileInvitesByProfileIdAsync(string profileId);
        Task<List<Profile>> GetProfilesByPrivateRunIdAsync(string privateRunId);
        Task UpdatePrivateRunAsync(PrivateRun privateRun);
        Task RemovePrivateRunAsync(string privateRunId);
    }
}