using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class TaskCommentAddEditDto
    {
        [Required]
        [MinLength(50, ErrorMessage = "Comment must be at least 50 characters")]
        public string CommentText { get; set; }
    }
    public class TaskCommentGetDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string CommentText { get; set; }
        public string EmployeeName { get; set; }
        public DateTime CreatedAt { get; set; }

        public int AttachmentCount { get; set; }
    }
}
