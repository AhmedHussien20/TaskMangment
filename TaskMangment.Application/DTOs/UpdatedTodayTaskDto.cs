

using TaskMangment.Application.ApiRequests;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class UpdatedTodayTaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public WorkTaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdatedTodayTasksRequest : BaseApiRequest
    {
        public PeriodDto Period { get; set; } = new();
    }


}
