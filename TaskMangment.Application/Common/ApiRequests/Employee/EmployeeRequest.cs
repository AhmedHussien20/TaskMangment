using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Employee
{
    public class EmployeeRequest : BaseApiRequest
    {
        public int? RoleLevel { get; set; }
        public string? PermissionCode { get; set; }
        public int? BranchId { get; set; }
        public bool? IsActive { get; set; }
        /// <summary>When true, only employees with a role that CanBeBranchManager.</summary>
        public bool? CanBeBranchManager { get; set; }
        /// <summary>When true, only employees with a role that RequiresBranchScope (area managers).</summary>
        public bool? RequiresBranchScope { get; set; }
    }

}
