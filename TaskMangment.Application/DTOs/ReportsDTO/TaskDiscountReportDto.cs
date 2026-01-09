using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class TaskDiscountReportDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string AssignedBy { get; set; } 
        public DateTime? ClosedDate { get; set; }
        public string Status { get; set; }
        public decimal AutoDiscount { get; set; } 
        public decimal ManualDiscount { get; set; } 
        public string Evaluation { get; set; }

        public string EmployeeName { get; set; }

    }

    public class TaskDiscountReportFilterDto
    {
        public int EmployeeId { get; set; }
        public WorkTaskStatus? Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public TaskMovementType MovementType { get; set; }
    }
   

}
