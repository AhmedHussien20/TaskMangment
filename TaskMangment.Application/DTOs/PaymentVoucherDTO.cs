using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class PaymentVoucherAddEditDto
    {
        [Required] public int CompanyId { get; set; }
        public int? BranchId { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int? CreatedByEmployeeId { get; set; }
    }

    public class PaymentVoucherGetDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string CreatedByName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AttachmentCount { get; set; }
    }
}
