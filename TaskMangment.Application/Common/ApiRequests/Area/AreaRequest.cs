namespace TaskMangment.Application.ApiRequests.Area
{
    public class AreaRequest : BaseApiRequest
    {
        public string? Name { get; set; }
        public int? CompanyId { get; set; }
    }

}
