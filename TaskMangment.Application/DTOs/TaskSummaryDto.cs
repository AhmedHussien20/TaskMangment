using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class TaskSummaryDto
    {
        public int MyTasks { get; set; }
        public int CreatedByMe { get; set; }
        public int InProgressTasks { get; set; }
        public int NewTasks { get; set; }
        public int ArchiveTasks { get; set; }
    }

}
