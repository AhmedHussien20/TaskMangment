using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Role
{
    public class RoleAssignmentReguest: BaseApiRequest
    {
        public bool? IsAssigned { set; get; }
    }
}
