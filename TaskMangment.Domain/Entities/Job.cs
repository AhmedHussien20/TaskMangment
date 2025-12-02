using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Job : BaseEntity
    {
        public int? DepartmentId { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; }
        [MaxLength(1000)] public string Description { get; set; }

        [ForeignKey(nameof(DepartmentId))] public Department Department { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
