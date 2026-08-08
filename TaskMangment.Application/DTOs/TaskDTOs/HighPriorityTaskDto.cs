using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class HighPriorityTaskDto
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = "";
        public List<string> Employees { get; set; } = new List<string>();
        public WorkTaskStatus Status { get; set; }
        public string StatusText { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
    public class TasksHighPriorityRequest : BaseApiRequest
    {
        public PeriodDto Period { get; set; }
    }


}
