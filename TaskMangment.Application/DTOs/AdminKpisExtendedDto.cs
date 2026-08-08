using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class AdminKpisExtendedDto
    {
        public double AverageCompletionHours { get; set; }
        public double OnTimeRatePercent { get; set; }
        public int HighPriorityOpenTasks { get; set; }
        public decimal PenaltiesThisMonth { get; set; }
    }

}
