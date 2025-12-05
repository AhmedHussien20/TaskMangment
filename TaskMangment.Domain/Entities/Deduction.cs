using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Deduction : BaseEntity
    {
         
        [Required] public int EmployeeId { get; set; }
        public int? DeductionTypeId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        public string Reason { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(EmployeeId))] public Employee Employee { get; set; }
        [ForeignKey(nameof(DeductionTypeId))] public DeductionType DeductionType { get; set; }
    }
}
