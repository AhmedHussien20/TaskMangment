using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class TaskPercentageAddEditDto
    {
        [Required]
        public string AchievementPercent { get; set; }
    }

    public class TaskPercentageGetDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string AchievementPercent { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
