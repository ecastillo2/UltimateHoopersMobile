using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class RequestDetailViewModelDto : RequestViewModelDto
    {
        public Request? Request { get; set; }
        public IList<Request>? RequestList { get; set; }
       


    }
}
