using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class CursorPaginatedResultDto<T>
    {
        /// <summary>
        /// The data items for this page
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Cursor for the next page (null if no more pages)
        /// </summary>
        public string NextCursor { get; set; }

        /// <summary>
        /// Indicates if there are more pages available
        /// </summary>
        public bool HasMore => !string.IsNullOrEmpty(NextCursor);

        /// <summary>
        /// Direction of the current pagination
        /// </summary>
        public string Direction { get; set; } = "next";

        /// <summary>
        /// Field used for sorting
        /// </summary>
        public string SortBy { get; set; } = "CreatedDate";

        /// <summary>
        /// Number of items in this page
        /// </summary>
        public int Count => Items?.Count ?? 0;
    }
}
