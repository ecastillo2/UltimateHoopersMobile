// File: WebAPI/DTOs/CommonDTOs.cs
using System.Collections.Generic;

namespace WebAPI.DTOs
{
    /// <summary>
    /// Generic error response
    /// </summary>
    public class ErrorDto
    {
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Generic success message response
    /// </summary>
    public class MessageDto
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Paginated result
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}