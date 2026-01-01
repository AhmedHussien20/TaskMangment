using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskCloseApproveEvent
    {
        public int RequestId { get; }
        public int TaskId { get; }
        public string TaskTitle { get; }
        public List<int> AssignedEmployeeIds { get; }

        public TaskCloseApproveEvent(int requestId, int taskId, string taskTitle, List<int> assignedEmployeeIds)
        {
            RequestId = requestId;
            TaskId = taskId;
            TaskTitle = taskTitle;
            AssignedEmployeeIds = assignedEmployeeIds;
        }
    }
}
