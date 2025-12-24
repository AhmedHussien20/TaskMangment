using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskUnAssignedEvent
    {
        public int TaskId { get; }
        public string TaskTitle { get; }
        public List<int> UnAssignedEmployeeIds { get; }

        public TaskUnAssignedEvent(int taskId, string taskTitle, List<int> unAssignedEmployeeIds)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            UnAssignedEmployeeIds = unAssignedEmployeeIds;
        }
    }

}
