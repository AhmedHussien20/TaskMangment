using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class CompletedTodayEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string? EmployeeImageUrl { get; set; } 

        public int CompletedTasksCount { get; set; }
    }

}
