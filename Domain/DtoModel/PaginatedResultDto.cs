using System;
using System.Collections.Generic;

namespace Domain.DtoModel
{
    /// <summary>
    /// A generic class to handle paginated results from the API
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated result</typeparam>
    public class PaginatedResultDto<T>
    {
        /// <summary>
        /// The collection of items for the current page
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The cursor for the next page, if any
        /// </summary>
        public string NextCursor { get; set; }

        /// <summary>
        /// The cursor for the previous page, if any
        /// </summary>
        public string PrevCursor { get; set; }

        /// <summary>
        /// Creates a new instance of PaginatedResultDto
        /// </summary>
        public PaginatedResultDto()
        {
            Items = new List<T>();
        }
    }
}