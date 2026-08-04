using System.Collections.Generic;

namespace TaskMangment.Domain.Event
{
    public class TaskAssignedEvent
    {
        public int TaskId { get; }
        public string TaskTitle { get; }
        /// <summary>Subject who was assigned (the person listeners care about).</summary>
        public int SubjectEmployeeId { get; }
        public string SubjectEmployeeName { get; }
        /// <summary>Subject + role listeners (excludes actor).</summary>
        public List<int> RecipientIds { get; }
        public string BranchName { get; }

        public TaskAssignedEvent(
            int taskId,
            string taskTitle,
            int subjectEmployeeId,
            string subjectEmployeeName,
            List<int> recipientIds,
            string? branchName = null)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            SubjectEmployeeId = subjectEmployeeId;
            SubjectEmployeeName = subjectEmployeeName ?? string.Empty;
            RecipientIds = recipientIds ?? new List<int>();
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }

        /// <summary>Backward-compatible alias used by older call sites/handlers.</summary>
        public List<int> AssignedEmployeeIds => RecipientIds;
    }
}
