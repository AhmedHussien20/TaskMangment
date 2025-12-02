using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        [Required] public int RoleId { get; set; }
        [Required] public int PermissionId { get; set; }

        [ForeignKey(nameof(RoleId))] public Role Role { get; set; }
        [ForeignKey(nameof(PermissionId))] public Permission Permission { get; set; }
    }
}
