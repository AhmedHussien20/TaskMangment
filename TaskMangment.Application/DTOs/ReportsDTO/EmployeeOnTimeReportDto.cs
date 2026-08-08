using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeOnTimeReportDto
    {
        public string EmployeeName { get; set; }

        public int TotalClosedTasks { get; set; }
        public int OnTimeTasks { get; set; }
        public int LateTasks { get; set; }

        public decimal CommitmentPercentage { get; set; }
    }


}
