using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Domain.Entities
{
    public class Task : BaseEntity
    {
        [Required] public int CompanyId { get; set; }
        public int? CreatedByEmployeeId { get; set; }

        [Required, MaxLength(300)] public string Title { get; set; }
        public string Description { get; set; }

        public int? CommentAllowPeriodDays { get; set; }
        public int MaxWarnings { get; set; } = 3;
        [Column(TypeName = "decimal(18,2)")] public decimal PenaltyAtMaxWarnings { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal PenaltyOnAutoClose { get; set; } = 0;
        public bool IsShared { get; set; } = false;
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public DateTime? DueDate { get; set; }
        public int? AssignedByEmployeeId { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CompanyId))] public Company Company { get; set; }
        public Employee CreatedBy { get; set; }
        public Employee AssignedBy { get; set; }

        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<Attachment> Attachments { get; set; }= new List<Attachment>();
        public ICollection<TaskCloseRequest> CloseRequests { get; set; } = new List<TaskCloseRequest>();
        public ICollection<TaskExtensionRequest> ExtensionRequests { get; set; }= new List<TaskExtensionRequest>();
    }
}
