using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Task
{
    public class TaskRequest: BaseApiRequest
    {
        public List<int>? EmployeeIds { get; set; }
        public int? StatusId { get; set; }
    }
}
