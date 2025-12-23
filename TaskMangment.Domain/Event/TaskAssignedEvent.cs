using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskAssignedEvent
    {
        public int TaskId { get; }
        public string TaskTitle { get; }
        public List<int> AssignedEmployeeIds { get; }

        public TaskAssignedEvent( int taskId, string taskTitle,List<int> assignedEmployeeIds)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            AssignedEmployeeIds = assignedEmployeeIds;
        }
    }


}