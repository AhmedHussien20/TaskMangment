using System.Collections.Generic;

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
        public string BranchName { get; }

        public TaskAchievePercentEvent(
            int percentId,
            string percent,
            int taskId,
            string taskTitle,
            string employeeName,
            List<int> recipients,
            string? branchName = null)
        {
            PercentId = percentId;
            Percent = percent;
            TaskId = taskId;
            TaskTitle = taskTitle;
            EmployeeName = employeeName;
            Recipients = recipients;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
