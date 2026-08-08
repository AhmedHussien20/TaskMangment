using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeAssignmentsReportDto
    {
        public string EmployeeName { get; set; }

        public int TotalTasks { get; set; }
        public int NewTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int ClosedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int ClosingSoonTasks { get; set; }

        public decimal CompletionRate { get; set; }  
    }


}
