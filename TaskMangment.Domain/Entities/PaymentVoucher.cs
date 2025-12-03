using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class PaymentVoucher : BaseEntity
    {
        [Required] public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        public int? CreatedByEmployeeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CompanyId))] public Company Company { get; set; }
        [ForeignKey(nameof(BranchId))] public Branch Branch { get; set; }
        [ForeignKey(nameof(CreatedByEmployeeId))] public Employee CreatedBy { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
