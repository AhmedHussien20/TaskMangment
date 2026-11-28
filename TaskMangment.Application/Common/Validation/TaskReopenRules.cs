using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Common.Validation
{
    /// <summary>
    /// Reopen only changes task status, close stamps, and assignment IsClosed.
    /// It does not read or write discounts, warnings, or penalties.
    /// </summary>
    public static class TaskReopenRules
    {
        public static bool IsReopenable(WorkTaskStatus status)
            => status is WorkTaskStatus.Closed or WorkTaskStatus.AutoClose;

        public static TaskReopenHistory Capture(
            WorkTask task,
            int reopenedByEmployeeId,
            DateTime? newDueDate,
            DateTime reopenedAt)
        {
            return new TaskReopenHistory
            {
                TaskId = task.Id,
                ReopenedByEmployeeId = reopenedByEmployeeId,
                ReopenedAt = reopenedAt,
                PreviousStatus = task.Status,
                PreviousClosedAt = task.ClosedAt,
                PreviousCloseReason = task.CloseReason,
                PreviousDueDate = task.DueDate,
                NewDueDate = newDueDate
            };
        }

        public static void ApplyOpenState(
            WorkTask task,
            IEnumerable<TaskAssignment> assignments,
            DateTime? newDueDate)
        {
            if (newDueDate.HasValue)
                task.DueDate = newDueDate;

            task.Status = WorkTaskStatus.InProgress;
            task.ClosedAt = null;
            task.ClosedByUserId = null;
            task.CloseReason = null;

            foreach (var assignment in assignments)
                assignment.IsClosed = false;
        }
    }
}
