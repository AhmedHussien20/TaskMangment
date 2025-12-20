using Microsoft.AspNetCore.Http;
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
        [MinLength(2, ErrorMessage = "Comment must be at least 2 characters")]
        public string CommentText { get; set; }
        public IFormFile? File { get; set; }

    }
    public class TaskCommentGetDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }

        public string CommentText { get; set; }
        public string EmployeeName { get; set; }
        public DateTime CreatedDate { get; set; }

        public int AttachmentCount { get; set; }
    }
}
