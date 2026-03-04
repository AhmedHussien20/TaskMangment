using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class CompletedTaskDetailDto
    {
        public int? TaskId { get; set; }
        public string TaskTitle { get; set; } = "";
        public List<string> EmployeeNames { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime ClosedAt { get; set; }
        public int DurationHours { get; set; }
    }

}
