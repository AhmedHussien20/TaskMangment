using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class PerformanceSummaryDto
    {
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public double CompletionRate =>
            TotalTasks == 0 ? 0 : (double)CompletedTasks / TotalTasks * 100;
    }

}
