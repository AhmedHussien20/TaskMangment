using System;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeTotalDiscountReportFilterDto
    {
        public int? BranchId { get; set; }
        public int? RoleId { get; set; }
        public string? RoleTitle { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public WorkTaskStatus? Status { get; set; }
    }

    public class EmployeeTotalDiscountReportRowDto
    {
        public string EmployeeName { get; set; }
        public decimal TotalDiscount { get; set; }
        public string RoleTitle { get; set; }
    }
}
