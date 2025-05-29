using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class CreateJoinedRunDto
    {
        public string? JoinedRunId { get; set; }
        public string? ProfileId { get; set; }
        public string? RunId { get; set; }
        public DateTime? InvitedDate { get; set; }
        public string? AcceptedInvite { get; set; }
        public string? Type { get; set; }
        public bool? Present { get; set; }
        public string? SquadId { get; set; }

        // Convert to JoinedRun entity
        public JoinedRun ToJoinedRun()
        {
            return new JoinedRun
            {
                JoinedRunId = JoinedRunId,
                ProfileId = ProfileId,
                RunId = RunId,
                InvitedDate = InvitedDate,
                AcceptedInvite = AcceptedInvite,
                Type = Type,
                Present = Present,
                SquadId = SquadId
            };
        }

        /// <summary>
        /// Constructor with required fields
        /// </summary>
        public CreateJoinedRunDto(string runId, string profileId)
        {
            RunId = runId;
            ProfileId = profileId;
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public CreateJoinedRunDto() { }
    }
}
