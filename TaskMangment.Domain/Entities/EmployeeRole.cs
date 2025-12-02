using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class EmployeeRole : BaseEntity
    {
        [Required] public int EmployeeId { get; set; }
        [Required] public int RoleId { get; set; }

        [ForeignKey(nameof(EmployeeId))] public Employee Employee { get; set; }
        [ForeignKey(nameof(RoleId))] public Role Role { get; set; }
    }
}
