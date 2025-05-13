using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Setting
    {
        [Key]
        public string? SettingId { get; set; }
        public string? ProfileId { get; set; }
        public bool AllowComments { get; set; }
        public bool ShowGameHistory { get; set; }
        public bool AllowEmailNotification { get; set; }

    }
}
