using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Department : BaseEntity
    {
        [Required] public int BranchId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; }
        public int? ManagerEmployeeId { get; set; } 

        // Navigation
        [ForeignKey(nameof(BranchId))] public Branch Branch { get; set; }
        [ForeignKey(nameof(ManagerEmployeeId))] public Employee Manager { get; set; }
        public ICollection<Job> Jobs { get; set; } = new List<Job>();

    }
}
