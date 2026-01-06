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

        public ICollection<Leave> Leaves { get; set; }
    }

}
