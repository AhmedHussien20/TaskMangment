using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskExtendApproveEvent
    {
        public int TaskId { get; }
        public string TaskTitle { get; }

        public DateTime? OldDueDate { get; }
        public DateTime? NewDueDate { get; }
        public List<int> AssignedEmployeeIds { get; }

        public TaskExtendApproveEvent(int taskId, string taskTitle, DateTime? oldDueDate, DateTime? newDueDate, List<int> assignedEmployeeIds)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            NewDueDate = newDueDate;
            OldDueDate = oldDueDate;
            AssignedEmployeeIds = assignedEmployeeIds;
        }
    }
}
