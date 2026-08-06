 
using System.ComponentModel.DataAnnotations; 
using TaskMangment.Domain.Entities; 

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class TaskAddEditDto
    {
        [Required]
        public List<int> AssignedEmployeeIds { get; set; } = new();

        [Required, MaxLength(300)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? CommentAllowPeriodDays { get; set; }

        /// <summary>Minimum comments required within each allow period (default 1).</summary>
        public int MinCommentsPerPeriod { get; set; } = 1;

        public int MaxWarningsBeforeDiscount { get; set; } = 3;

        public int MaxWarnings { get; set; } = 3;

        public decimal PenaltyAtMaxWarnings { get; set; } = 0;

        public decimal PenaltyOnAutoClose { get; set; } = 0;
        public decimal PenaltyOnStopComment { get; set; }

        public bool IsShared { get; set; } = false;

        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.New;
        public DateTime? DueDate { get; set; }
        public bool requireUploadFile { get; set; }

    }
    public class TaskGetDto
    {
        public int Id { get; set; }               
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsShared { get; set; }        
        public DateTime CreatedDate { get; set; }     
        public string AssignedByName { get; set; }  
        public List<TaskEmployeeAssignmentDto> AssignEmployee { get; set; } = new(); 
        public TaskPriority Priority { get; set; }   
        public WorkTaskStatus Status { get; set; }    
        public DateTime? DueDate { get; set; }

        public string PriorityText => Priority.ToString();
        public string StatusText => Status.ToString();

        public int MaxWarningsBeforeDiscount { get; set; } = 3;
        public int MaxWarnings { get; set; } = 3;
        public decimal PenaltyAtMaxWarnings { get; set; } = 0;
        public decimal PenaltyOnAutoClose { get; set; } = 0;
        public decimal PenaltyOnStopComment { get; set; } = 0;
        public int? CommentAllowPeriodDays { get; set; }
        public int MinCommentsPerPeriod { get; set; } = 1;

        public DateTime? ClosedAt { get; set; }
        public int? ClosedByUserId { get; set; }
        public CloseReason? CloseReason { get; set; }
        public bool requireUploadFile { get; set; }
        public DateTime? NewDate { get; set; }
        public int? NumberOfExtensions { get; set; }
        public bool CreatedByMe { get; set; } = false;
        /// <summary>
        /// True only when the current user is CreatedBy or AssignedBy (not scope-inferred).
        /// </summary>
        public bool IsCreatorOrAssigner { get; set; } = false;
        /// <summary>Pending close + extension requests for this task.</summary>
        public int PendingRequestsCount { get; set; }

        /// <summary>Unread notifications for the current user on this task.</summary>
        public int UnreadNotificationsCount { get; set; }

    }



    public class TaskEmployeeAssignmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TaskAssignmentDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public bool IsRead { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Mobile { get; set; }


    }

    public class TaskRequestsDto
    {
        public int TaskId { get; set; }
        public List<TaskExtensionRequestDetailsDto> ExtensionRequests { get; set; } = new();
        public List<TaskCloseRequestDetailsDto> CloseRequests { get; set; } = new();
    }


}
