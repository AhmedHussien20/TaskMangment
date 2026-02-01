using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.ChartsDTOs
{
    public class EmpTasksChartResultDto
    {
        public List<EmpTaskChartItem> Tasks { get; set; } = new();
        public List<TaskStatusCountDto> StatusCounts { get; set; } = new();
    }



    public class EmpTaskChartItem
    {
        public string TaskName { get; set; }
        public string? Percent { get; set; }
    }

    public class TaskStatusCountDto
    {
        public WorkTaskStatus Status { get; set; }
        public int Count { get; set; }
    }

}
