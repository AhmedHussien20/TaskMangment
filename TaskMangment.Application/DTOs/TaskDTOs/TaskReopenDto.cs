using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class ReopenTaskDto
    {
        /// <summary>Required when the current due date is already in the past.</summary>
        public DateTime? DueDate { get; set; }
    }

    public class TaskReopenHistoryDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ReopenedByEmployeeId { get; set; }
        public string ReopenedByName { get; set; }
        public DateTime ReopenedAt { get; set; }
        public WorkTaskStatus PreviousStatus { get; set; }
        public DateTime? PreviousClosedAt { get; set; }
        public CloseReason? PreviousCloseReason { get; set; }
        public DateTime? PreviousDueDate { get; set; }
        public DateTime? NewDueDate { get; set; }
    }
}
