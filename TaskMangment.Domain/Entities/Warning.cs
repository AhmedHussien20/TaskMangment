using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Warning : BaseEntity
    {
        [Required] public int TaskAssignmentId { get; set; }
        public int? IssuedByEmployeeId { get; set; }
        [MaxLength(1000)] public string Reason { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TaskAssignmentId))] public TaskAssignment TaskAssignment { get; set; }
        [ForeignKey(nameof(IssuedByEmployeeId))] public Employee IssuedBy { get; set; }
    }
}
