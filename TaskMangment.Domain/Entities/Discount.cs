using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Discount : BaseEntity
    {

        [Required] public int EmployeeId { get; set; }
        public int? TaskId { get; set; }
        [MaxLength(1000)] public string Reason { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        public int? CreatedByEmployeeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EmployeeId))] public Employee Employee { get; set; }
        [ForeignKey(nameof(TaskId))] public WorkTask Task { get; set; }
        [ForeignKey(nameof(CreatedByEmployeeId))] public Employee CreatedBy { get; set; }
    }
}
