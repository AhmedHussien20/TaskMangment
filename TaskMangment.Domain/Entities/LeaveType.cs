using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class LeaveType : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string NameAr { get; set; }

        [Required]
        [MaxLength(200)]
        public string NameEn { get; set; }

        public bool IsPaid { get; set; }

        public int? MaxDaysPerYear { get; set; }

        /// <summary>Training schema only: owning trainee copy, or 0 for the template. Ignored in dbo.</summary>
        public int CompanyId { get; set; }

        public ICollection<Leave> Leaves { get; set; }
    }

}
