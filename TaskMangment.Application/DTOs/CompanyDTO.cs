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

        public int TechnicalManagerId { get; set; }
        public int FinancialManagerId { get; set; }
    }

    public class CompanyGetDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public int TechnicalManagerId { get; set; }
        public int FinancialManagerId { get; set; }
    }
}
