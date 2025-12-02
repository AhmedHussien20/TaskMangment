using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Company : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }
        [MaxLength(50)] public string Phone { get; set; }
        [MaxLength(50)] public string Mobile { get; set; }
        [MaxLength(50)] public string Fax { get; set; }
        [MaxLength(200)] public string Email { get; set; }
        [MaxLength(200)] public string Website { get; set; }
        [MaxLength(100)] public string CommercialRecord { get; set; }

        // ✅ FK to Employee
        public int? TechnicalManagerId { get; set; }
        public int? FinancialManagerId { get; set; }

        [ForeignKey(nameof(TechnicalManagerId))]
        public Employee TechnicalManager { get; set; }

        [ForeignKey(nameof(FinancialManagerId))]
        public Employee FinancialManager { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
