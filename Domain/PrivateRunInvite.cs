using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class PrivateRunInvite
    {
        [Key]
        public string? PrivateRunInviteId { get; set; }
        public string? ProfileId { get; set; }
        public string? PrivateRunId { get; set; }       
        public string? InvitedDate { get; set; }
        public string? AcceptedInvite { get; set; }
        public string? Type { get; set; }
        public bool? Present { get; set; }
        public string? SquadId { get; set; }

        [NotMapped]
        public Profile? InviteProfile { get; set; }
        [NotMapped]
        public string? UserName { get; set; }
        [NotMapped]
        public string? ImageURL { get; set; }

        [NotMapped]
        public string? FirstName { get; set; }

        [NotMapped]
        public string? LastName { get; set; }
        [NotMapped]
        public string? SubId { get; set; }
    }
}
