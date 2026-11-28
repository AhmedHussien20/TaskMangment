using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class BranchTasksReportFilterDto
    {
        public int BranchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public WorkTaskStatus? Status { get; set; }
    }

    public class EmployeeMiniDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class BranchTaskReportRowDto
    {
        public int TaskId { get; set; }        
        public List<int> MergedTaskIds { get; set; } = new();

        public string Title { get; set; } = "";
        public string AssignedBy { get; set; } = "";

        public DateTime CreatedDate { get; set; }

        public DateTime? OriginalDueDate { get; set; }
        public DateTime? EffectiveDueDate { get; set; }
        public int ExtensionRequestsCount { get; set; }

        public string StatusText { get; set; } = "";

        public List<EmployeeMiniDto> Employees { get; set; } = new();
    }
}

