using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class SettingUpdateModelDto
    {
        public bool AllowComments { get; set; }
        public bool ShowGameHistory { get; set; }
        public bool AllowEmailNotification { get; set; }

        public void UpdateSetting(Setting setting)
        {
            setting.AllowComments = AllowComments;
            setting.ShowGameHistory = ShowGameHistory;
            setting.AllowEmailNotification = AllowEmailNotification;
        }
    }
}
