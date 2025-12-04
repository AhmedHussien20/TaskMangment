using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class AuditLogAddDto
    {
        [Required] public string EntityName { get; set; }
        public int? EntityId { get; set; }
        [Required] public string Action { get; set; }
        [Required] public string ChangedBy { get; set; }
        public string Details { get; set; }
    }

    public class AuditLogListDto
    {
        public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Details { get; set; }
    }

    public class AuditLogDetailsDto
    {
        public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Details { get; set; }
    }
}
