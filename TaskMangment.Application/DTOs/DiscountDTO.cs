using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class DiscountAddEditDto
    {
        [Required]
        public int EmployeeId { get; set; }


        [Required, MaxLength(1000)]
        public string Reason { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }

    public class DiscountListDto
    {
        public string EmployeeName { get; set; }
        public string? TaskTitle { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DiscountDetailsDto
    {
        public string EmployeeName { get; set; }
        public string? TaskTitle { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
