using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class SettingViewModelDto
    {
        // Parameterless constructor for JSON deserialization
        [JsonConstructor]
        public SettingViewModelDto() { }

        // Original constructor
        public SettingViewModelDto(Setting setting)
        {
            SettingId = setting.SettingId;
            ProfileId = setting.ProfileId;
            AllowComments = setting.AllowComments;
            ShowGameHistory = setting.ShowGameHistory;
            AllowEmailNotification = setting.AllowEmailNotification;
        }

        public string SettingId { get; set; }
        public string ProfileId { get; set; }
        public bool AllowComments { get; set; }
        public bool ShowGameHistory { get; set; }
        public bool AllowEmailNotification { get; set; }
    }
}
