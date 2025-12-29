using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class AdminKpiDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal TotalPenaltiesThisMonth { get; set; }
        public int WarningsThisMonth { get; set; }
    }

}
