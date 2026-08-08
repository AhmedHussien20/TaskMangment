using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class AdminKpiDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int NewTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal TotalPenaltiesThisMonth { get; set; }
        public int WarningsThisMonth { get; set; }
    }

    public class TaskStatusDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public WorkTaskStatus Status { get; set; }
        public string StatusText { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string> Employees { get; set; }
    }
    public class PeriodDto
    {
        public DashboardPeriod Type { get; set; }
        public DateTime? StartDate { get; set; }  
        public DateTime? EndDate { get; set; } 
    }
    public class TasksByStatusRequest : BaseApiRequest
    {
        public PeriodDto Period { get; set; }
    }

}
