using System;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class EmployeeTaskCommentRowDto
    {
        public int CommentId { get; set; }
        public DateTime CommentDate { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }
}

