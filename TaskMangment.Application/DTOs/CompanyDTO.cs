using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class CompanyAddEditDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(50)]
        public string Mobile { get; set; }

        [MaxLength(50)]
        public string Fax { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Website { get; set; }

        [MaxLength(100)]
        public string CommercialRecord { get; set; }

        public int? TechnicalManagerId { get; set; }
        public int? FinancialManagerId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CompanyGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string CommercialRecord { get; set; }

        // Names of the managers
        public string TechnicalManagerName { get; set; }
        public string FinancialManagerName { get; set; }

        public bool IsActive { get; set; }
    }
}
