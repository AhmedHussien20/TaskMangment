using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeArchivedTasksReportDto
    {
        public string EmployeeName { get; set; }

        public int TotalTasks { get; set; }           
        public int ArchivedTasksCount { get; set; }  

        public decimal ArchiveRate { get; set; }     
    }


}
