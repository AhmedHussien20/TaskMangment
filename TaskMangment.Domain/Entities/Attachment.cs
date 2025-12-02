using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Attachment : BaseEntity
    {
        public int? TaskId { get; set; }
        public int? CommentId { get; set; }
        public int? VoucherId { get; set; }
        [Required, MaxLength(300)] public string FileName { get; set; }
        [Required, MaxLength(1000)] public string FilePath { get; set; } // e.g. S3 key or server path
        [MaxLength(100)] public string ContentType { get; set; }
        public long? Size { get; set; }
        public int? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TaskId))] public Task Task { get; set; }
        [ForeignKey(nameof(CommentId))] public TaskComment Comment { get; set; }
        [ForeignKey(nameof(UploadedBy))] public Employee UploadedByEmployee { get; set; }
    }
}
