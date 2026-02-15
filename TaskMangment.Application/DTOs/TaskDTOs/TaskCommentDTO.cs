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
        [MinLength(2, ErrorMessage = "Comment must be at least 2 characters")]
        public string? CommentText { get; set; }
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

    public class AttachmentVm
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string Url { get; set; } = "";

        public string UrlDownload { get; set; } = "";
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class DownloadFileResultVm
    {
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = "file";
        public string ContentType { get; set; } = "application/octet-stream";
    }


}
