using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class CalendarEvent:BaseEntity
    {
        public int? CompanyId { get; set; }
        [Required, MaxLength(300)] public string Title { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AllDay { get; set; } = false;
        public int? RelatedTaskId { get; set; }
        public int? CreatedByEmployeeId { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CompanyId))] public Company Company { get; set; }
        [ForeignKey(nameof(RelatedTaskId))] public WorkTask RelatedTask { get; set; }
        [ForeignKey(nameof(CreatedByEmployeeId))] public Employee CreatedBy { get; set; }
    }
}
