namespace TaskMangment.Application.ApiRequests.Platform
{
    public class PlatformAdminListRequest : BaseApiRequest
    {
        public bool? IsActive { get; set; }
    }

    public class OrganizationListRequest : BaseApiRequest
    {
        public bool? IsActive { get; set; }
    }

    public class TraineeCopyListRequest : BaseApiRequest
    {
        public bool? IsActive { get; set; }
    }
}
