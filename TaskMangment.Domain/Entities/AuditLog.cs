using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
         
        [Required] public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; } // Create, Update, Delete, etc.
        public string ChangedBy { get; set; } // username or employee id
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } // optional JSON diff

        /// <summary>Training schema only: the trainee copy the entry belongs to. Ignored in dbo.</summary>
        public int CompanyId { get; set; }
    }
}
