using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class CursorPaginatedResultDto<T>
    {
        public List<T> Items { get; set; }
        public string NextCursor { get; set; }
        public bool HasMore { get; set; }
        public string Direction { get; set; }
        public string SortBy { get; set; }
    }
}
