using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeTaskTrackingReportDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty; 
        public DateTime CreatedDate { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string AssignedBy { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
    }
}
