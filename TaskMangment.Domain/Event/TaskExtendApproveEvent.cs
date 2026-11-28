using System;
using System.Collections.Generic;

namespace TaskMangment.Domain.Event
{
    public class TaskExtendApproveEvent
    {
        public int RequestId { get; }
        public int TaskId { get; }
        public string TaskTitle { get; }

        public DateTime? OldDueDate { get; }
        public DateTime? NewDueDate { get; }
        public List<int> AssignedEmployeeIds { get; }
        public string BranchName { get; }

        public TaskExtendApproveEvent(
            int requestId,
            int taskId,
            string taskTitle,
            DateTime? oldDueDate,
            DateTime? newDueDate,
            List<int> assignedEmployeeIds,
            string? branchName = null)
        {
            RequestId = requestId;
            TaskId = taskId;
            TaskTitle = taskTitle;
            NewDueDate = newDueDate;
            OldDueDate = oldDueDate;
            AssignedEmployeeIds = assignedEmployeeIds;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
