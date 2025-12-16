using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class TaskExtensionRequest : BaseEntity
    {

        public int TaskId { get; set; }
        [Required] 
        public int TaskAssignmentId { get; set; }
        public int? RequestedByEmployeeId { get; set; }
        public DateTime NewDueDate { get; set; }
        [MaxLength(1000)] 
        public string Reason { get; set; }
        public ExtensionRequestStatus Status { get; set; } = ExtensionRequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public int? ReviewedByEmployeeId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [ForeignKey(nameof(TaskAssignmentId))] public TaskAssignment TaskAssignment { get; set; }
        [ForeignKey(nameof(RequestedByEmployeeId))] public Employee RequestedBy { get; set; }
        [ForeignKey(nameof(ReviewedByEmployeeId))] public Employee ReviewedBy { get; set; }
    }
}
