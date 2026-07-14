using System;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class DateRangeReportFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? RoleId { get; set; }
        public string? RoleTitle { get; set; }
    }

    public class ReportRoleOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
    }
}
