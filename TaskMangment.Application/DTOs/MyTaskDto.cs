
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class MyTaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public WorkTaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal ProgressPercent { get; set; }
        public bool IsOverdue => DueDate != null && DueDate < DateTime.UtcNow;
    }

}
