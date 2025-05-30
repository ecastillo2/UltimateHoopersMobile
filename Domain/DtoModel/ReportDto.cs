using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ReportDto
    {
        public int CourtsCount { get; set; } = 0;
        public int ProductsCount { get; set; } = 0;
        public int ClientsCount { get; set; } = 0;
        public int RunsCount { get; set; } = 0;
        public int UsersCount { get; set; } = 0;
        public int ProfilesCount { get; set; } = 0;
        public int OrdersCount { get; set; } = 0;
        public int PostsCount { get; set; } = 0;

       

        // Optional: Add properties to indicate if counts are reliable
        public bool IsDataComplete { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
