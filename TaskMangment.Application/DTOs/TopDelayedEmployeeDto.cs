using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class TopDelayedEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DelayedTasksCount { get; set; }
    }
}
