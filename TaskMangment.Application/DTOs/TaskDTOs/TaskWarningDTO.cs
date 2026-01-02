using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class WarningAddEditDto
    {
        [Required]
        public int IssuedEmployeeId { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; }
    }

    public class WarningGetDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public string TaskTitle { get; set; }

        public int TaskAssignmentId { get; set; }
        public string Reason { get; set; }
        public DateTime IssuedAt { get; set; }

        public string IssuedEmployeeName { get; set; }
        public string IssuedByName { get; set; }


    }

   
}
