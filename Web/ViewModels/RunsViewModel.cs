using Domain.DtoModel;

namespace Website.ViewModels
{
    public class RunsViewModel
    {
        public IEnumerable<RunDetailViewModelDto> Runs { get; set; }
        public string NextCursor { get; set; }
        public string PreviousCursor { get; set; }
        public bool HasMore { get; set; }
        public int TotalCount { get; set; }
        public int CurrentLimit { get; set; }
        public string CurrentSortBy { get; set; }
        public string UserRole { get; set; }
        public string ProfileId { get; set; }
    }
}
