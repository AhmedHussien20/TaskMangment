using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class EmployeeKpiDto
    {
        public int MyActiveTasks { get; set; }
        public int DueSoonTasks { get; set; }
        public int MyWarnings { get; set; }
        public decimal MyPenalties { get; set; }
    }

    public class WarningDto
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskStatus { get; set; } = string.Empty;
    }

    public class DeductionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool AutoDiscount { get; set; }
        public DiscountType DiscountType { get; set; }
    }

}
