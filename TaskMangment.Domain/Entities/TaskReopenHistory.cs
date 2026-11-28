using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMangment.Domain.Entities
{
    /// <summary>
    /// One row per reopen. Close fields are copied here before they are cleared on the task.
    /// Discounts, warnings, and penalties are never stored or changed by this record.
    /// </summary>
    public class TaskReopenHistory : BaseEntity
    {
        public int TaskId { get; set; }
        public int ReopenedByEmployeeId { get; set; }
        public DateTime ReopenedAt { get; set; }

        public WorkTaskStatus PreviousStatus { get; set; }
        public DateTime? PreviousClosedAt { get; set; }
        public CloseReason? PreviousCloseReason { get; set; }
        public DateTime? PreviousDueDate { get; set; }
        public DateTime? NewDueDate { get; set; }

        [ForeignKey(nameof(TaskId))]
        public WorkTask Task { get; set; }

        [ForeignKey(nameof(ReopenedByEmployeeId))]
        public Employee ReopenedBy { get; set; }
    }
}
