using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Branch : BaseEntity
    {
        [Required] public int CompanyId { get; set; }
        public int? AreaId { get; set; }

        [Required, MaxLength(200)] public string Name { get; set; }
        [MaxLength(500)] public string Address { get; set; }
        [MaxLength(50)] public string Phone { get; set; }
        [MaxLength(50)] public string Mobile { get; set; }
        [MaxLength(50)] public string Fax { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        [ForeignKey(nameof(CompanyId))] public Company Company { get; set; }
        [ForeignKey(nameof(AreaId))] public Area Area { get; set; }
        public ICollection<Department> Departments { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
