using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.ReportDTOs
{
    public class TaskReportDto
    {
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public string AssignedTo { get; set; } = "";
        public DateTime? DueDate { get; set; }
    }

}
