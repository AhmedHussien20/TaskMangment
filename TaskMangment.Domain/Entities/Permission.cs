using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Permission : BaseEntity
    {
        [Required, MaxLength(100)] public string Code { get; set; }    // e.g. CREATE_TASK
        [Required, MaxLength(200)] public string Name { get; set; }
        [MaxLength(500)] public string Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
