using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class TaskCloseRequest : BaseEntity
    {
        [Required] 
        public int TaskAssignmentId { get; set; }
        [MaxLength(1000)] 
        public string Message { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public CloseRequestStatus Status { get; set; } = CloseRequestStatus.Pending;
        public int? ReviewedByEmployeeId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [ForeignKey(nameof(TaskAssignmentId))] public TaskAssignment TaskAssignment { get; set; }
        [ForeignKey(nameof(ReviewedByEmployeeId))] public Employee ReviewedBy { get; set; }
    }

}
