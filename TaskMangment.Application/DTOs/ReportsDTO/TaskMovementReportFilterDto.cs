using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class TaskMovementReportFilterDto
    {
        public int? EmployeeId { get; set; }
        public TaskMovementType MovementType { get; set; }
        public string ReportTitle { get; set; } = string.Empty;
    }
    public class TaskMovementReportDto
    {
        public string ReportTitle { get; set; }
        public TaskMovementType MovementType { get; set; }

        public string TaskTitleWithId { get; set; }
        public string AssignedBy { get; set; }
        public string CommentedBy { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
    }

    public enum TaskMovementType
    {
        Outgoing = 1, // صادرة
        Incoming = 2  // واردة
    }


}
