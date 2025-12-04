using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class TaskAssignmentAddEditDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public bool IsResponsible { get; set; } = false;
    }
    public class TaskAssignmentGetDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string TaskTitle { get; set; }

        public decimal ProgressPercent { get; set; }
        public int WarningsCount { get; set; }
        public bool IsClosed { get; set; }
    }

}
