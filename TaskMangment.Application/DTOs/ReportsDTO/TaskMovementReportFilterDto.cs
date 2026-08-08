using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class TaskMovementReportFilterDto
    {
        public int? EmployeeId { get; set; }
        public TaskMovementType MovementType { get; set; }
        public string ReportTitle { get; set; } = string.Empty;
        public WorkTaskStatus? Status { get; set; }
    }
    public class TaskMovementReportDto
    {
        public int TaskId { get; set; }
        public string ReportTitle { get; set; }
        public TaskMovementType MovementType { get; set; }

        public string TaskTitleWithId { get; set; }
        public string AssignedBy { get; set; }
        public string CommentedBy { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
        public List<EmployeeBriefDto> AssignedTo { get; set; } = new();

    }
    public class EmployeeBriefDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }


    public enum TaskMovementType
    {
        Outgoing = 1, // صادرة
        Incoming = 2  // واردة
    }


}
