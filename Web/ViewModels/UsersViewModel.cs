using Domain.DtoModel;
using System.Collections.Generic;
using System.Linq;

namespace Website.ViewModels
{
    public class UsersViewModel
    {
        // Properties for the view
        public List<UserDetailViewModelDto> Users { get; set; } = new List<UserDetailViewModelDto>();

        // Pagination properties
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string NextCursor { get; set; }
        public string PreviousCursor { get; set; }
        public int PageSize { get; set; } = 20; // Default value

        // Additional properties
        public string CurrentSortBy { get; set; } = "Points";
        public string SearchTerm { get; set; }
        public string FilterOption { get; set; }

        // Static mapping method
        public static UsersViewModel FromPaginatedResult(CursorPaginatedResultDto<UserDetailViewModelDto> result, string sortBy = "Points", int limit = 20)
        {
            if (result == null)
                return new UsersViewModel { CurrentSortBy = sortBy, PageSize = limit };

            var viewModel = new UsersViewModel
            {
                Users = result.Items?.ToList() ?? new List<UserDetailViewModelDto>(),
                PageSize = limit,
                CurrentSortBy = sortBy
            };

            // Try to get pagination properties if they exist
            // Use reflection to safely check for properties
            var resultType = result.GetType();

            var hasNextPageProp = resultType.GetProperty("HasNextPage");
            if (hasNextPageProp != null)
                viewModel.HasNextPage = (bool)hasNextPageProp.GetValue(result);

            var hasPreviousPageProp = resultType.GetProperty("HasPreviousPage");
            if (hasPreviousPageProp != null)
                viewModel.HasPreviousPage = (bool)hasPreviousPageProp.GetValue(result);

            var nextCursorProp = resultType.GetProperty("NextCursor");
            if (nextCursorProp != null)
                viewModel.NextCursor = (string)nextCursorProp.GetValue(result);

            // For previous cursor, we'll try common variations
            var prevCursorProp = resultType.GetProperty("PreviousCursor") ??
                                resultType.GetProperty("PrevCursor") ??
                                resultType.GetProperty("PrevPageCursor");
            if (prevCursorProp != null)
                viewModel.PreviousCursor = (string)prevCursorProp.GetValue(result);

            return viewModel;
        }
    }
}