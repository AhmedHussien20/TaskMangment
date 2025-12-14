using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class RolePermissionBulkAssignDto
    {
        [Required]
        public List<PermissionAssignmentDto> Assignments { get; set; } = new();
    }

    public class PermissionAssignmentDto
    {
        [Required]
        public int PermissionId { get; set; }

        [Required]
        public bool Assign { get; set; }
    }

    public class AssignedPermissionDto
    {
        public int PermissionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsAssigned { get; set; }
    }

}
