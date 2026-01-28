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
        public int? EmployeeId { get; set; }
        public WorkTaskStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public TaskMovementType MovementType { get; set; }
    }
    public class EmployeeDiscountAuditGroupDto
    {
        public string EmployeeName { get; set; }

        public List<TaskDiscountAuditRowDto> Tasks { get; set; } = new();

        public decimal TotalAutoDiscount => Tasks.Sum(x => x.AutoDiscount);
        public decimal TotalManualDiscount => Tasks.Sum(x => x.ManualDiscount);
        public decimal TotalDiscount => Tasks.Sum(x => x.TotalDiscount);
        public int TasksCount => Tasks.Count;
    }

    public class TaskDiscountAuditRowDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Status { get; set; }

        public decimal AutoDiscount { get; set; }
        public decimal ManualDiscount { get; set; }

        public string EmployeeName { get; set; }        
        public string GroupEmployeeName { get; set; }

        public decimal TotalDiscount => AutoDiscount + ManualDiscount;
    }

    public class TaskDiscountAuditReportDto
    {
        public TaskMovementType MovementType { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<EmployeeDiscountAuditGroupDto> Groups { get; set; } = new();

        public decimal GrandTotalAuto => Groups.Sum(g => g.TotalAutoDiscount);
        public decimal GrandTotalManual => Groups.Sum(g => g.TotalManualDiscount);
        public decimal GrandTotal => Groups.Sum(g => g.TotalDiscount);
        public int TotalTasks => Groups.Sum(g => g.TasksCount);
    }

}
