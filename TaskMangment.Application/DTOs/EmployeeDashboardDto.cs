using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class EmployeeDashboardDto
    {
        public EmployeeKpiDto Kpis { get; set; }
        public List<MyTaskDto> MyTasks { get; set; }
        public PerformanceSummaryDto Performance { get; set; }
        // public List<NotificationDto> Notifications { get; set; }
    }
}
