using System.Collections.Generic;

namespace TaskMangment.Domain.Event
{
    public class TaskUnAssignedEvent
    {
        public int TaskId { get; }
        public string TaskTitle { get; }
        /// <summary>Subject who was unassigned (the person listeners care about).</summary>
        public int SubjectEmployeeId { get; }
        public string SubjectEmployeeName { get; }
        /// <summary>Subject + role listeners (excludes actor).</summary>
        public List<int> RecipientIds { get; }

        public TaskUnAssignedEvent(
            int taskId,
            string taskTitle,
            int subjectEmployeeId,
            string subjectEmployeeName,
            List<int> recipientIds)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            SubjectEmployeeId = subjectEmployeeId;
            SubjectEmployeeName = subjectEmployeeName ?? string.Empty;
            RecipientIds = recipientIds ?? new List<int>();
        }

        /// <summary>Backward-compatible alias.</summary>
        public List<int> UnAssignedEmployeeIds => RecipientIds;
    }
}
