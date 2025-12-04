using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace TaskMangment.Application.DTOs
{
    public class AttachmentAddDto
    {
        [Required] public IFormFile File { get; set; }
    public int? TaskId { get; set; }
    public int? CommentId { get; set; }
    public int? VoucherId { get; set; }
    public int? UploadedBy { get; set; }
}

public class AttachmentGetDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long? Size { get; set; }
    public int? TaskId { get; set; }
    public int? CommentId { get; set; }
    public int? VoucherId { get; set; }
    public int? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
}
