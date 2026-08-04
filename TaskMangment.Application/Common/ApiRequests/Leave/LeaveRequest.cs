using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Leave
{
    public class LeaveRequest : BaseApiRequest
    {
        public List<int>? EmployeeIds { get; set; }
        public int? StatusId { get; set; }

        /// <summary>
        /// When true, list leaves for employees in the caller's AccessScope (like ViewScopedTasks),
        /// not only RoleNotificationSource subjects.
        /// </summary>
        public bool? ViewScopedLeaves { get; set; }
    }
}
