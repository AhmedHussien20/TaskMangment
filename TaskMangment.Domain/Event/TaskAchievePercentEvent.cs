using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskAchievePercentEvent
    {
        public int PercentId { get; }

        public string Percent { get; }
        public int TaskId { get; }
        public string TaskTitle { get; set; }
        public string EmployeeName { get; set; }
        public List<int> Recipients { get; }

        public TaskAchievePercentEvent(int percentId, string percent, int taskId, string taskTitle, string employeeName, List<int> recipients)
        {
            PercentId = percentId;
            Percent = percent;
            TaskId = taskId;
            TaskTitle = taskTitle;
            EmployeeName = employeeName;
            Recipients = recipients;
        }
    }
}
