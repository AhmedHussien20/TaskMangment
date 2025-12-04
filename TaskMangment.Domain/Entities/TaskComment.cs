using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class TaskComment : BaseEntity
    {
        [Required] 
        public int TaskId { get; set; }
        public int? EmployeeId { get; set; }
        [Required] 
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TaskId))] 
        public WorkTask Task { get; set; }
        [ForeignKey(nameof(EmployeeId))] 
        public Employee Employee { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
