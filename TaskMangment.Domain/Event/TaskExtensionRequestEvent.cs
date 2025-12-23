using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskExtensionRequestEvent
    {
        public int RequestId { get; }
        public int TaskId { get; }
        public string taskTitle { get; set; }
        public string EmployeeName { get; set; }
        public List<int> Recipients { get; }

        public TaskExtensionRequestEvent(int requestId, int taskId, string employeeName, List<int> recipients, string taskTitle)
        {
            RequestId = requestId;
            TaskId = taskId;
            EmployeeName = employeeName;
            Recipients = recipients;
            this.taskTitle = taskTitle;
        }
    }
}
