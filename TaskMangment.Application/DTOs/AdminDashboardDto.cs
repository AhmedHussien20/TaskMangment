using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class AdminDashboardDto
    {
        public AdminKpiDto Kpis { get; set; }
        public TaskStatusChartDto TaskStatus { get; set; }
        public List<TopDelayedEmployeeDto> TopDelayedEmployees { get; set; }
    }

}
