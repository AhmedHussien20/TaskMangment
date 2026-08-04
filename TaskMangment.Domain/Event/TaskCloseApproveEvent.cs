using System.Collections.Generic;

namespace TaskMangment.Domain.Event
{
    public class TaskCloseApproveEvent
    {
        public int RequestId { get; }
        public int TaskId { get; }
        public string TaskTitle { get; }
        public List<int> AssignedEmployeeIds { get; }
        public string BranchName { get; }

        public TaskCloseApproveEvent(
            int requestId,
            int taskId,
            string taskTitle,
            List<int> assignedEmployeeIds,
            string? branchName = null)
        {
            RequestId = requestId;
            TaskId = taskId;
            TaskTitle = taskTitle;
            AssignedEmployeeIds = assignedEmployeeIds;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
