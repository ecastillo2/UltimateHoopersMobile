using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ProfileDetailViewModelDto : ProfileViewModelDto
    {
        public SettingViewModelDto Setting { get; set; }
        public ScoutingReportViewModelDto ScoutingReport { get; set; }
        public GameStatistics GameStatistics { get; set; }
        public string FollowersCount { get; set; }
        public string FollowingCount { get; set; }

        public ProfileDetailViewModelDto(Profile profile) : base(profile)
        {
            FollowersCount = profile.FollowersCount;
            FollowingCount = profile.FollowingCount;
        }
    }
}
