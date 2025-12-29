using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class PendingCloseRequestTaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
    }

}
