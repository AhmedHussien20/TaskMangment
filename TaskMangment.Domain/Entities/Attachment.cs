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
        [Required, MaxLength(300)] 
        public string FileName { get; set; }
        [Required, MaxLength(1000)] 
        public string FilePath { get; set; } // e.g. S3 key or server path
        [MaxLength(100)] 
        public string ContentType { get; set; }
        public int ReferenceId { get; set; }
        
        public int ReferenceType { get; set; } 
        public AttachmentType AttachmentType { get; set; }
        public long? Size { get; set; }
        public int? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsUploadedToBlob { get; set; }
        public string? BlobUrl { get; set; }
        public DateTime? BlobUploadedAt { get; set; }
        public string? BlobUploadError { get; set; }

        [ForeignKey(nameof(UploadedBy))] public Employee UploadedByEmployee { get; set; }
    }
}
