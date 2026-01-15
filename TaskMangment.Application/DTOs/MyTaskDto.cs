
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class MyTaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public WorkTaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ProgressPercent { get; set; }
        public bool IsOverdue => DueDate != null && DueDate < DateTime.UtcNow;
    }

    public class TodayCommentTaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public WorkTaskStatus Status { get; set; }
        public string StatusText { get; set; }

        public DateTime? DueDate { get; set; }

        public string AssignedBy { get; set; }
        public List<string> Employees { get; set; }
    }


}
