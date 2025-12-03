using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class RoleAddDto
    {
        public int? CompanyId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class RoleGetDto
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class RolePermissionAssignDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; }
    }

    public class AssignRoleToEmployeeDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }

    public class RoleWithPermissionsDto
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public List<PermissionGetDto> Permissions { get; set; }
    }
    public class PermissionAddDto
    {
        [Required, MaxLength(100)]
        public string Code { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class PermissionGetDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }


}
