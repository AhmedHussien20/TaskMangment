using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class TaskAssignment : BaseEntity
    {
        [Required] 
        public int TaskId { get; set; }
        [Required] 
        public int EmployeeId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsResponsible { get; set; } = false;
        [Column(TypeName = "decimal(5,2)")] 
        public decimal ProgressPercent { get; set; } = 0;
        public int WarningsCount { get; set; } = 0;
        public bool IsClosed { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(TaskId))] 
        public WorkTask Task { get; set; }
        [ForeignKey(nameof(EmployeeId))] 
        public Employee Employee { get; set; }

        public ICollection<Warning> Warnings { get; set; } = new List<Warning>();
        public ICollection<TaskExtensionRequest> ExtensionRequests { get; set; }= new List<TaskExtensionRequest>();
        public ICollection<TaskCloseRequest> CloseRequests { get; set; } = new List<TaskCloseRequest>();
    }
}
