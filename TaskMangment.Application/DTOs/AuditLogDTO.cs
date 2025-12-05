using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class AuditLogDTO
    {
        [Required] public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }
        public string ChangedBy { get; set; } 
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; } // optional JSON diff
    }
}
