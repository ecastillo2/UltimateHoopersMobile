using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class VideoDetailViewModelDto : VideoViewModelDto
    {
        public Video Video { get; set; }
        public IList<Video> VideoList { get; set; }
       


    }
}
