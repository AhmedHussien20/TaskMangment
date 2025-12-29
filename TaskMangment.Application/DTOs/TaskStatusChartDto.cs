using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class TaskStatusChartDto
    {
        public int New { get; set; }
        public int InProgress { get; set; }
        public int Closed { get; set; }
        public int Archived { get; set; }
    }

}
