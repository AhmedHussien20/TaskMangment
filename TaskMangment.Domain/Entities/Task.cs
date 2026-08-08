 
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TaskMangment.Domain.Entities
{
    public class WorkTask : BaseEntity
    {
        [Required] public int CompanyId { get; set; }
        public int? CreatedByEmployeeId { get; set; }

        [Required, MaxLength(300)] public string Title { get; set; }
        public string Description { get; set; }

        public CommentAllowPeriod? CommentAllowPeriodDays { get; set; }
        /// <summary>Minimum comments required within each CommentAllowPeriodDays window.</summary>
        public int MinCommentsPerPeriod { get; set; } = 1;
        public int MaxWarningsBeforeDiscount { get; set; } = 3;
        public int MaxWarnings { get; set; } = 3;
        [Column(TypeName = "decimal(18,2)")] public decimal PenaltyAtMaxWarnings { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal PenaltyOnAutoClose { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal PenaltyOnStopComment { get; set; } = 0;

        public bool IsShared { get; set; } = false;
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public DateTime? DueDate { get; set; }
        public int? AssignedByEmployeeId { get; set; }
        public WorkTaskStatus  Status { get; set; } = WorkTaskStatus.New;
        public DateTime? ClosedAt { get; set; }
        public int? ClosedByUserId { get; set; }
        public CloseReason? CloseReason { get; set; }

        public bool requireUploadFile { get; set; } = false;


        [ForeignKey(nameof(CompanyId))] public Company Company { get; set; }

        [ForeignKey(nameof(CreatedByEmployeeId))]
        public Employee CreatedBy { get; set; }

        [ForeignKey(nameof(AssignedByEmployeeId))]
        public Employee AssignedBy { get; set; }

        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
       // public ICollection<Attachment> Attachments { get; set; }= new List<Attachment>();
        public ICollection<TaskCloseRequest> CloseRequests { get; set; } = new List<TaskCloseRequest>();
        public ICollection<TaskExtensionRequest> ExtensionRequests { get; set; }= new List<TaskExtensionRequest>();
    }
}
