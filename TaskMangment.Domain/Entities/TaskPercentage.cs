using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class TaskPercentage: BaseEntity
    {

        [Required]
        public int TaskId { get; set; }
        public int? EmployeeId { get; set; }

        [Required]
        public string AchievementPercent { get; set; }

        [ForeignKey(nameof(TaskId))]
        public WorkTask Task { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; }
    }
}
