using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Role : BaseEntity
    {
        public int? CompanyId { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; }
        [MaxLength(500)] public string Description { get; set; }

        [ForeignKey(nameof(CompanyId))] public Company Company { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
        public ICollection<EmployeeRole> EmployeeRoles { get; set; }
    }
}
