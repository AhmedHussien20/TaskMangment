using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class EmployeeKpiDto
    {
        public int MyActiveTasks { get; set; }
        public int DueSoonTasks { get; set; }
        public int MyWarnings { get; set; }
        public decimal MyPenalties { get; set; }
    }

}
