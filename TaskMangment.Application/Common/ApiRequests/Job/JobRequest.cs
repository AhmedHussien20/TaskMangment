using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Job
{
    public class JobRequest : BaseApiRequest
    {
        public int? DepartmentId { get; set; }
        public int? EmployeeId { get; set; }
        public string? Title { get; set; }
    }
}
