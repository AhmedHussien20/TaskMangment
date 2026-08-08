using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class CopyTaskDto
    {
        public int TaskId { get; set; }
        public DateTime NewDueDate { get; set; }
    }
}
