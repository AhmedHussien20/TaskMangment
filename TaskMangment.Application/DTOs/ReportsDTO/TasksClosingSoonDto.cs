using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class TasksClosingSoonDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? ClosedDate { get; set; }
        public WorkTaskStatus Status { get; set; }
        public string EmployeeName { get; set; }

    }
}
