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
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        /// <summary>Optional legacy hierarchy. Not used for authorization; kept for future use.</summary>
        public int Level { get; set; } = 10;

        /// <summary>When true, assignees must configure ManagerBranches coverage.</summary>
        public bool RequiresBranchScope { get; set; }

        /// <summary>When true, assignees must configure one EmployeeType coverage.</summary>
        public bool RequiresEmployeeTypeScope { get; set; }

        public Company Company { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    }

}
