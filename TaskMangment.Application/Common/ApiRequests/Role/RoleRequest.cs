using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Role
{
    public class RoleRequest : BaseApiRequest
    {
        public int? CompanyId { get; set; }
        public string? Name { get; set; }
    }

    public class PermissionRequest : BaseApiRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
