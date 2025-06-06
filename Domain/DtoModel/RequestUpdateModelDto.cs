
namespace Domain.DtoModel
{
    public class RequestUpdateModelDto
    {
        public string? RequestId { get; set; }
        public string? RunId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }

        public void UpdateRequest(Request request)
        {
            request.RequestId = RequestId;
            request.RunId = RunId;
            request.ProfileId = ProfileId;
            request.Status = Status;
            request.CreatedDate = CreatedDate;

        }
    }
}

