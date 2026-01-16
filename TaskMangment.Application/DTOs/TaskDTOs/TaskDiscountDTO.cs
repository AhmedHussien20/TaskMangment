using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs.TaskDTOs
{
    public class DiscountAddEditDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Reason { get; set; }

    }

    public class DiscountGetDto
    {
        public int TaskId { get; set; }
        public string EmployeeName { get; set; }
        public string? TaskTitle { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool AutoDiscount { get; set; }
        public DiscountType DiscountType { get; set; }

    }


}
