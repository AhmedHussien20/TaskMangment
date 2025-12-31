 
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

        public int MaxWarnings { get; set; } = 3;

        public decimal PenaltyAtMaxWarnings { get; set; } = 0;

        public decimal PenaltyOnAutoClose { get; set; } = 0;

        public bool IsShared { get; set; } = false;

        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.New;


        public DateTime? DueDate { get; set; }
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

        public int MaxWarnings { get; set; } = 3;
        public decimal PenaltyAtMaxWarnings { get; set; } = 0;
        public decimal PenaltyOnAutoClose { get; set; } = 0;
        public int? CommentAllowPeriodDays { get; set; }
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
    }

}
